//Copyright 2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class VendorContactsService : BaseCoordinationService, IVendorContactsService
    {
        private readonly IVendorContactsRepository _vendorContactsRepository;
        private readonly IVendorsRepository _vendorsRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IPersonMatchingRequestsRepository _personMatchingRequestsRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;

        public VendorContactsService(

            IReferenceDataRepository referenceDataRepository,
            IVendorContactsRepository vendorContactsRepository,
            IVendorsRepository vendorsRepository,
            IPersonRepository personRepository,
            IPersonMatchingRequestsRepository personMatchingRequestsRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            //: base(adapterRegistry, configurationRepository, currentUserFactory, roleRepository, logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
            _vendorContactsRepository = vendorContactsRepository;
            _vendorsRepository = vendorsRepository;
            _personRepository = personRepository;
            _personMatchingRequestsRepository = personMatchingRequestsRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all vendor-contacts
        /// </summary>
        /// <returns>Collection of VendorContacts DTO objects</returns>
        public async Task<Tuple<IEnumerable<VendorContacts>, int>> GetVendorContactsAsync(int offset, int limit, Dtos.VendorContacts criteria, bool bypassCache = false)
        {

            var vendorContactsCollection = new List<Ellucian.Colleague.Dtos.VendorContacts>();
            IEnumerable<OrganizationContact> entities = new List<OrganizationContact>();
            string vendorId = string.Empty;

            //Parse for filter
            if (criteria != null && criteria.Vendor != null && !string.IsNullOrWhiteSpace(criteria.Vendor.Id))
            {
                try
                {
                    vendorId = await _vendorsRepository.GetVendorIdFromGuidAsync(criteria.Vendor.Id);
                    if (string.IsNullOrWhiteSpace(vendorId))
                    {
                        return new Tuple<IEnumerable<VendorContacts>, int>(new List<Ellucian.Colleague.Dtos.VendorContacts>(), 0);
                    }
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<VendorContacts>, int>(new List<Ellucian.Colleague.Dtos.VendorContacts>(), 0);
                }
            }

            var orgContacts = await _vendorContactsRepository.GetVendorContactsAsync(offset, limit, vendorId);

            if (orgContacts != null && orgContacts.Item1.Any())
            {
                var vendorIds = orgContacts.Item1.Where(v => !string.IsNullOrWhiteSpace(v.VendorId)).Select(i => i.VendorId).Distinct().ToArray();
                Dictionary<string, string> vendorGuidCollection = await _vendorsRepository.GetVendorGuidsCollectionAsync(vendorIds);

                foreach (var entity in orgContacts.Item1)
                {
                    VendorContacts dto = await ConvertEntityToDtoAsync(entity, vendorGuidCollection);
                    vendorContactsCollection.Add(dto);
                }
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            var totalCount = orgContacts.Item2;
            return vendorContactsCollection != null && vendorContactsCollection.Any() ?
                new Tuple<IEnumerable<VendorContacts>, int>(vendorContactsCollection, totalCount) :
                new Tuple<IEnumerable<VendorContacts>, int>(new List<Ellucian.Colleague.Dtos.VendorContacts>(), 0);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a VendorContacts from its GUID
        /// </summary>
        /// <returns>VendorContacts DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.VendorContacts> GetVendorContactsByGuidAsync(string guid, bool bypassCache = true)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a Vendor Contacts.");
            }

            try
            {
                var entity = await _vendorContactsRepository.GetGetVendorContactsByGuidAsync(guid);
                Dictionary<string, string> vendorGuidCollection = await _vendorsRepository.GetVendorGuidsCollectionAsync(new List<string>() { entity.VendorId });

                VendorContacts dto = await ConvertEntityToDtoAsync(entity, vendorGuidCollection);

                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }

                return dto;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No vendor-contacts was found for guid '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No vendor-contacts was found for guid '{0}'", guid), ex);
            }
            catch (RepositoryException e)
            {
                IntegrationApiExceptionAddError(e.Errors[0].Message, "GUID.Not.Found", guid, "", System.Net.HttpStatusCode.NotFound);
                throw IntegrationApiException;
            }
        }

        /// <summary>
        /// Converts entity to dto.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="vendorGuidCollection"></param>
        /// <returns></returns>
        private async Task<VendorContacts> ConvertEntityToDtoAsync(OrganizationContact source, Dictionary<string, string> vendorGuidCollection)
        {
            VendorContacts dto = new VendorContacts();
            dto.Contact = new Dtos.DtoProperties.VendorContactsContact();
            //id
            dto.Id = source.Guid;

            //vendor.id
            if (!string.IsNullOrEmpty(source.VendorId))
            {
                string venGuid = string.Empty;
                if (vendorGuidCollection != null && vendorGuidCollection.Any() && vendorGuidCollection.TryGetValue(source.VendorId, out venGuid))
                {
                    dto.Vendor = new GuidObject2(venGuid);
                }
                else
                {
                    IntegrationApiExceptionAddError(string.Concat("Vendor guid not found for Organization Contact Id: '", source.Id, "'"), "GUID.Not.Found", source.Guid, source.Id);
                }
            }
            else
            {
                IntegrationApiExceptionAddError(string.Concat("Vendor guid not found for Organization Contact Id: '", source.Id, "'"), "Bad.Data", source.Guid, source.Id );
            }
            //contact.relationshipType.id
            try
            {
                var relTypeId = await _referenceDataRepository.GetRelationTypes3GuidAsync(source.RelationshipType);
                if (relTypeId == null || string.IsNullOrEmpty(relTypeId.Item1))
                {
                    IntegrationApiExceptionAddError(string.Concat("Contact relation type guid not found for Organization Contact Id: '", source.Id, "'"), "GUID.Not.Found", source.Guid, source.Id );
                }
                else
                {
                    dto.Contact.RelationshipType = new GuidObject2(relTypeId.Item1);
                }
            }
            catch (Exception)
            {
                IntegrationApiExceptionAddError(string.Concat("Contact relation type guid not found for Organization Contact Id: '", source.Id, "'"), "GUID.Not.Found", source.Guid, source.Id );
            }

            //contact.person.detail.id, contact.person.name
            dto.Contact.Person = new Dtos.DtoProperties.VendorContactsPerson()
            {
                Detail = new GuidObject2(source.ContactPersonGuid),                
            };

            if (!string.IsNullOrWhiteSpace(source.ContactPreferedName))
            {
                dto.Contact.Person.Name = source.ContactPreferedName;
            }
            else
            {
                IntegrationApiExceptionAddError(string.Concat("Contact person name not found for Organization Contact Id: '", source.Id, "'"), "Bad.Data", source.Guid, source.Id );

            }
            //contact.phones.type.id, contact.phones.number, contact.phones.extension
            if (source.PhoneInfos != null && source.PhoneInfos.Any())
            {
                foreach (var phoneInfo in source.PhoneInfos)
                {
                    VendorContactsPhones contactPhone = new VendorContactsPhones();
                    try
                    {
                        if (!string.IsNullOrEmpty(phoneInfo.PhoneType))
                        {
                            var typeGuid = await _referenceDataRepository.GetPhoneTypesGuidAsync(phoneInfo.PhoneType);
                            if (!string.IsNullOrEmpty(typeGuid))
                            {
                                contactPhone.Type = new GuidObject2(typeGuid);
                            }
                            else
                            {
                                IntegrationApiExceptionAddError(string.Concat("Contact phone type guid not found for Organization Contact Id: '", source.Id, "'"), "GUID.Not.Found", source.Guid, source.Id );
                            }
                        }
                    }
                    catch (Exception)
                    {
                        IntegrationApiExceptionAddError(string.Concat("Contact phone type guid not found for Organization Contact Id: '", source.Id, "'"), "Bad.Data", source.Guid, source.Id );
                    }
                    if (!string.IsNullOrEmpty(phoneInfo.PhoneNumber))
                    {
                        contactPhone.Number = phoneInfo.PhoneNumber;
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Concat("Contact phone type guid not found for Organization Contact Id: '", source.Id, "'"), "Bad.Data", source.Guid, source.Id);
                    }

                    contactPhone.Extension = !string.IsNullOrEmpty(phoneInfo.PhoneExtension) ? phoneInfo.PhoneExtension : null;

                    if (dto.Contact.Phones == null) dto.Contact.Phones = new List<VendorContactsPhones>();

                    dto.Contact.Phones.Add(contactPhone);
                }
            }

            //contact.startOn
            dto.Contact.StartOn = source.StartDate.HasValue ? source.StartDate.Value : default(DateTime?);
            //contact.endOn
            dto.Contact.EndOn = source.EndDate.HasValue ? source.EndDate.Value : default(DateTime?);

            return dto;
        }

       

        #region vendor contact initiation process

        /// <summary>
        /// Provide a method for an external system to request to create a relationship between a vendor (CORP) and a person that could also initiate duplicate checking for the contact person.
        /// This API will allow POST of a vendor contact and will potentially create a RELATIONSHIP and ORGANIZATION.CONTACT record where OCN.VENDOR.CONTACT = 'Y'.
        /// </summary>
        /// <param name="vendorContactInitiationProcess"></param>
        /// <returns></returns>
        public async Task<object> CreateVendorContactInitiationProcessAsync(VendorContactInitiationProcess vendorContactInitiationProcess)
        {
            try
            {
                CheckCreateVendorContactInitiationProcessPermission();

                if(vendorContactInitiationProcess == null)
                {
                    IntegrationApiExceptionAddError("Must provide a vendor-contact-initiation-process object for create.", "Missing.Request.Body");
                    throw IntegrationApiException;
                }
                _vendorContactsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                OrganizationContactInitiationProcess entity = await ConvertVendorContactInitProcDtoToEntity(vendorContactInitiationProcess);

                //Create vendor contact relationship
                var vendContactRel = await _vendorContactsRepository.CreateVendorContactInitiationProcessAsync(entity);

                if (vendContactRel.Item1 != null && (vendContactRel.Item1.GetType()) == typeof(OrganizationContact))
                {
                    // return the newly created personRelationships
                    var vendorGuid = ((OrganizationContact)vendContactRel.Item1).VendorId; 
                    Dictionary<string, string> vendorGuidCollection = await _vendorsRepository.GetVendorGuidsCollectionAsync(new List<string>() { vendorGuid });
                    var dto = await ConvertEntityToDtoAsync((OrganizationContact)vendContactRel.Item1, vendorGuidCollection);
                    return dto;
                }
                else
                {
                    // return the newly created PersonMatchingRequests
                    var personMatchingRequest = await _personMatchingRequestsRepository.GetPersonMatchRequestsByIdAsync(vendContactRel.Item2);
                    Dictionary<string, string> personDict = await _personRepository.GetPersonGuidsCollectionAsync(new List<string>() { personMatchingRequest.PersonId });
                    personMatchingRequest.PersonDict = await _personRepository.GetPersonGuidsCollectionAsync(new List<string>() { personMatchingRequest.RecordKey });
                    //Leave following code here. When we revisit to consolidate in to adapter we can uncoment it.
                    //var personMatchingAdapter = new AutoMapperAdapter<Domain.Base.Entities.PersonMatchRequest, Ellucian.Colleague.Dtos.PersonMatchingRequests>(_adapterRegistry, logger);//PersonMatchingRequestDtoAdapter(_adapterRegistry, logger);
                    //var dto = personMatchingAdapter.MapToType(personMatchingRequest);
                    //return dto;
                    var dto = ConvertPersonMatchRequestEntityToDto(personMatchingRequest, personDict);
                    return dto;
                }
            }
            catch (RepositoryException ex)
            {
                throw;
            }
            catch (IntegrationApiException ex)
            {
                throw;
            }
            catch (PermissionsException e)
            {
                throw;// new PermissionsException(e.Message);
            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException(ex.Message, ex.InnerException);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<OrganizationContactInitiationProcess> ConvertVendorContactInitProcDtoToEntity(VendorContactInitiationProcess dto, bool bypassCache = false)
        {
            OrganizationContactInitiationProcess entity = null;
            string vendorId = null;
            string personId = null;
            string firstName = null;
            string lastName = null;
            string middleName = string.Empty;
            List<ContactPhoneInfo> phonesInfos = new List<ContactPhoneInfo>();
            ContactEmailInfo emailInfo = null;

            if (dto == null)
            {
                IntegrationApiExceptionAddError("Vendor contact initiation process body is required.", "Validation.Exception");
                throw IntegrationApiException;
            }
            else
            {
                //vendor.id
                if (dto.Vendor == null)
                {
                    IntegrationApiExceptionAddError("vendor is a required property.", "Missing.Required.Property");
                    throw IntegrationApiException;
                }

                if (dto.Vendor != null && string.IsNullOrWhiteSpace(dto.Vendor.Id))
                {
                    IntegrationApiExceptionAddError("vendor.id is a required property.", "Missing.Required.Property");
                    throw IntegrationApiException;
                }
                else
                {
                    try
                    {                        
                        vendorId = await _vendorsRepository.GetVendorIdFromGuidAsync(dto.Vendor.Id);
                        var isCorp = await _personRepository.IsCorpAsync( vendorId );
                        if ( !isCorp )
                        {
                            IntegrationApiExceptionAddError( "The vendor specified is a person, only those vendors setup as organizations are eligible for a separate contact person.", "Validation.Exception" );
                            throw IntegrationApiException;
                        }
                        if (string.IsNullOrWhiteSpace(vendorId))
                        {
                            IntegrationApiExceptionAddError("vendor.id is not a valid GUID for vendor.", "GUID.Not.Found"); 
                            throw IntegrationApiException;
                        }
                    }
                    catch( IntegrationApiException e )
                    {
                        throw;
                    }
                    catch( ArgumentException e )
                    {
                        IntegrationApiExceptionAddError( "vendor.id is not a valid GUID for vendor.", "GUID.Not.Found" );
                        throw IntegrationApiException;
                    }
                    catch( KeyNotFoundException e )
                    {
                        IntegrationApiExceptionAddError( "vendor.id is not a valid GUID for vendor.", "GUID.Not.Found" );
                        throw IntegrationApiException;
                    }
                }

                //vendorContact
                if (dto.VendorContact == null)
                {
                    IntegrationApiExceptionAddError("vendorContact is a required property.", "Missing.Required.Property");
                    throw IntegrationApiException;
                }

                //vendorContact.detail.id
                if (dto.VendorContact.Detail != null && string.IsNullOrWhiteSpace(dto.VendorContact.Detail.Id))
                {
                    IntegrationApiExceptionAddError("vendorContact.detail.id is a required property.", "Missing.Required.Property");                    
                }
                else if(dto.VendorContact.Detail != null && !string.IsNullOrWhiteSpace(dto.VendorContact.Detail.Id))
                {
                    try
                    {                        
                        personId = await _personRepository.GetPersonIdFromGuidAsync(dto.VendorContact.Detail.Id);
                        var isCorp = await _personRepository.IsCorpAsync( personId );
                        if ( isCorp )
                        {
                            IntegrationApiExceptionAddError( "You cannot associate a corporation to a person as their other relations.", "Validation.Exception" );
                            throw IntegrationApiException;
                        }

                        if ( string.IsNullOrWhiteSpace(personId))
                        {
                            IntegrationApiExceptionAddError("vendorContact.detail.id is not a valid GUID for vendor contact.", "GUID.Not.Found");
                            throw IntegrationApiException;
                        }
                    }
                    catch( IntegrationApiException e)
                    {
                        throw;
                    }
                    catch( ArgumentNullException e)
                    {
                        IntegrationApiExceptionAddError( "vendorContact.detail.id is not a valid GUID for vendor contact.", "GUID.Not.Found" );
                        throw IntegrationApiException;
                    }
                    catch(KeyNotFoundException e)
                    {
                        IntegrationApiExceptionAddError( "vendorContact.detail.id is not a valid GUID for vendor contact.", "GUID.Not.Found" );
                        throw IntegrationApiException;
                    }
                    catch( RepositoryException e )
                    {
                        IntegrationApiExceptionAddError( "vendorContact.detail.id is not a valid GUID for vendor contact.", "GUID.Not.Found" );
                        throw IntegrationApiException;
                    }
                }

                //vendorContact.person
                if (dto.VendorContact.Person == null && dto.VendorContact.Detail == null)
                {
                    IntegrationApiExceptionAddError("vendorContact.person is a required property.", "Missing.Required.Property");
                }

                if (dto.VendorContact.Person != null && string.IsNullOrWhiteSpace(dto.VendorContact.Person.FirstName))
                {
                    IntegrationApiExceptionAddError("vendorContact.person.firstName is a required property.", "Missing.Required.Property");
                }
                else
                {
                    firstName = dto.VendorContact.Person != null && !string.IsNullOrWhiteSpace(dto.VendorContact.Person.FirstName) ? dto.VendorContact.Person.FirstName : null;
                }

                if (dto.VendorContact.Person != null && string.IsNullOrWhiteSpace(dto.VendorContact.Person.LastName))
                {
                    IntegrationApiExceptionAddError("vendorContact.person.lastName is a required property.", "Missing.Required.Property");
                }
                else
                {
                    lastName = dto.VendorContact.Person != null && !string.IsNullOrWhiteSpace(dto.VendorContact.Person.LastName) ? dto.VendorContact.Person.LastName : null;
                }

                if(dto.VendorContact.Person != null && !string.IsNullOrWhiteSpace(dto.VendorContact.Person.MiddleName))
                {
                    middleName = dto.VendorContact.Person != null && !string.IsNullOrWhiteSpace(dto.VendorContact.Person.MiddleName)? dto.VendorContact.Person.MiddleName : null;
                }

                //vendorContact.person.phones
                if (dto.VendorContactPhones != null && dto.VendorContactPhones.Any())
                {
                    List<Domain.Base.Entities.PhoneType> phoneTypes = null;
                    try
                    {
                        phoneTypes = (await _referenceDataRepository.GetPhoneTypesAsync(bypassCache)).ToList();
                    }
                    catch
                    {
                        IntegrationApiExceptionAddError("Phone types not found.", "Validation.Exception");
                    }

                    if (phoneTypes == null || !phoneTypes.Any())
                    {
                        IntegrationApiExceptionAddError("Phone types not found.", "Validation.Exception");
                    }
                    else
                    {
                        foreach (var phone in dto.VendorContactPhones)
                        {
                            ContactPhoneInfo phInfo = new ContactPhoneInfo();

                            //type
                            if (phone != null && (phone.Type == null || string.IsNullOrWhiteSpace(phone.Type.Id)))
                            {
                                IntegrationApiExceptionAddError("vendorContactPhones.type.id, the phone type is required for a vendor contact phone number.", "Missing.Required.Property");
                            }
                            else if (phone != null && (phone.Type != null && !string.IsNullOrWhiteSpace(phone.Type.Id)))
                            {
                                var phoneTypeCode = phoneTypes.FirstOrDefault(pt => pt.Guid.Equals(phone.Type.Id, StringComparison.InvariantCultureIgnoreCase));
                                if (phoneTypeCode == null)
                                {
                                    IntegrationApiExceptionAddError(string.Format("Phone type not found for guid {0}.", phone.Type.Id), "Validation.Exception");
                                }
                                else
                                {
                                    phInfo.PhoneType = phoneTypeCode.Code;
                                }
                            }

                            //number
                            if (phone != null && string.IsNullOrWhiteSpace(phone.Number))
                            {
                                IntegrationApiExceptionAddError("vendorContactPhones.number is a required property.", "Missing.Required.Property");
                            }
                            else
                            {
                                phInfo.PhoneNumber = phone.Number;
                            }

                            //extension
                            phInfo.PhoneExtension = !string.IsNullOrWhiteSpace(phone.Extension) ? phone.Extension : null;
                            phonesInfos.Add(phInfo);
                        }
                    }
                }

                //email
                if (dto.VendorContactEmail != null)
                {
                    if (emailInfo == null) emailInfo = new ContactEmailInfo();
                    if (string.IsNullOrWhiteSpace(dto.VendorContactEmail.EmailAddress))
                    {
                        IntegrationApiExceptionAddError("vendorContact.person.email.address is a required property.", "Missing.Required.Property");
                    }
                    else
                    {
                        emailInfo.EmailAddress = dto.VendorContactEmail.EmailAddress;
                    }

                    if ((dto.VendorContactEmail.EmailType == null) || (dto.VendorContactEmail.EmailType != null && string.IsNullOrWhiteSpace(dto.VendorContactEmail.EmailType.Id)))
                    {
                        IntegrationApiExceptionAddError("vendorContact.person.email.type.id is a required property.", "Missing.Required.Property");
                    }
                    else if (dto.VendorContactEmail.EmailType != null && !string.IsNullOrWhiteSpace(dto.VendorContactEmail.EmailType.Id))
                    {
                        List<Domain.Base.Entities.EmailType> emailTypes = new List<Domain.Base.Entities.EmailType>();
                        try
                        {
                            emailTypes = (await _referenceDataRepository.GetEmailTypesAsync(bypassCache)).ToList();
                            if (emailTypes == null || !emailTypes.Any())
                            {
                                IntegrationApiExceptionAddError("Email types not found.", "Validation.Exception");
                            }
                            else
                            {
                                var emailType = emailTypes.FirstOrDefault(et => et.Guid.Equals(dto.VendorContactEmail.EmailType.Id, StringComparison.InvariantCultureIgnoreCase));
                                if (emailType == null)
                                {
                                    IntegrationApiExceptionAddError(string.Format("Email type not found for guid {0}.", dto.VendorContactEmail.EmailType.Id), "Validation.Exception");
                                }
                                else
                                {
                                    emailInfo.EmailType = emailType.Code;
                                }
                            }
                        }
                        catch
                        {
                            IntegrationApiExceptionAddError("Email types not found.", "Validation.Exception");
                        }
                    }
                }
                entity = new OrganizationContactInitiationProcess()
                {
                    VendorId = vendorId,
                    PersonId = personId,
                    FirstName = firstName,
                    MiddleName = middleName,
                    LastName = lastName,
                    PhoneInfos = phonesInfos,
                    EmailInfo = emailInfo,
                    RequestType = "VENCONTACT"
                };
            }
            //Collection of errors returned.
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            return entity;
        }

        /// <summary>
        /// Converts entity to dto.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="personDict"></param>
        /// <returns></returns>
        private Dtos.PersonMatchingRequests ConvertPersonMatchRequestEntityToDto(PersonMatchRequest source, Dictionary<string, string> personDict)
        {
            Dtos.PersonMatchingRequests dto = new Dtos.PersonMatchingRequests();

            //id
            if (string.IsNullOrEmpty(source.Guid))
            {
                IntegrationApiExceptionAddError("Could not find a GUID for person-matching-requests entity.", "Bad.Data", id: source.RecordKey);
            }
            dto.Id = source.Guid;
            if (!string.IsNullOrEmpty(source.Originator))
            {
                dto.Originator = source.Originator;
            }

            //prospect.id
            string personGuid;
            if (!string.IsNullOrEmpty(source.PersonId))
            {
                if (!personDict.TryGetValue(source.PersonId, out personGuid))
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate guid for person ID '{0}'", source.RecordKey), "Bad.Data", source.Guid, source.RecordKey);
                }
                dto.Person = new Dtos.GuidObject2(personGuid);
            }

            //outcomes
            if (source.Outcomes != null && source.Outcomes.Any())
            {
                foreach (var outcome in source.Outcomes)
                {
                    if (outcome != null)
                    {
                        try
                        {
                            if (dto.Outcomes == null)
                            {
                                dto.Outcomes = new List<Dtos.PersonMatchingRequestsOutcomesDtoProperty>();
                            }
                            dto.Outcomes.Add(new Dtos.PersonMatchingRequestsOutcomesDtoProperty()
                            {
                                Type = (Dtos.EnumProperties.PersonMatchingRequestsType)Enum.Parse(typeof(Dtos.EnumProperties.PersonMatchingRequestsType), outcome.Type.ToString()),
                                Status = (Dtos.EnumProperties.PersonMatchingRequestsStatus)Enum.Parse(typeof(Dtos.EnumProperties.PersonMatchingRequestsStatus), outcome.Status.ToString()),
                                Date = outcome.Date.DateTime
                            });
                        }
                        catch
                        {
                            IntegrationApiExceptionAddError(string.Format("Unable to translate outcome type of '{0}' or status of '{1}'.", outcome.Type.ToString(), outcome.Status.ToString()), "Bad.Data", source.Guid, source.RecordKey);
                        }
                    }
                }
            }

            return dto;
        }

        /// <summary>
        /// Helper method to determine if the user has permission to create/update Vendor Contacts.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckCreateVendorContactInitiationProcessPermission()
        {
            bool hasPermission = HasPermission(ColleagueFinancePermissionCodes.ProcessVendorContact);

            // User is not allowed to create or update Vendor Contacts without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to create or update vendor-contact-initiation-process.");
            }
        }

        #endregion
    }
}