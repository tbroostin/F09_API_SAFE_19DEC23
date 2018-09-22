//Copyright 2016-2017 Ellucian Company L.P. and its affiliates

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Domain.Exceptions;
using VendorType = Ellucian.Colleague.Dtos.EnumProperties.VendorType;
using Ellucian.Colleague.Dtos.Filters;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class VendorsService : BaseCoordinationService, IVendorsService
    {
        private readonly IColleagueFinanceReferenceDataRepository _colleagueFinanceReferenceDataRepository;
        private readonly IVendorsRepository _vendorsRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IInstitutionRepository _institutionRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public VendorsService(
            IColleagueFinanceReferenceDataRepository colleagueFinanceReferenceDataRepository,
            IVendorsRepository vendorsRepository,
            IPersonRepository personRepository,
            IInstitutionRepository institutionRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository:configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _colleagueFinanceReferenceDataRepository = colleagueFinanceReferenceDataRepository;
            _vendorsRepository = vendorsRepository;
            _personRepository = personRepository;
            _institutionRepository = institutionRepository;
        }


        private IEnumerable<Domain.ColleagueFinance.Entities.VendorTerm> _vendorTerms = null;
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.VendorTerm>> GetAllVendorTermsAsync(bool bypassCache)
        {
            if (_vendorTerms == null)
            {
                _vendorTerms = await _colleagueFinanceReferenceDataRepository.GetVendorTermsAsync(bypassCache);
            }
            return _vendorTerms;
        }

        private IEnumerable<Domain.ColleagueFinance.Entities.VendorType> _vendorTypes = null;
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.VendorType>> GetAllVendorTypesAsync(bool bypassCache)
        {
            if (_vendorTypes == null)
            {
                _vendorTypes = await _colleagueFinanceReferenceDataRepository.GetVendorTypesAsync(bypassCache);
            }
            return _vendorTypes;
        }

        private IEnumerable<Domain.ColleagueFinance.Entities.VendorHoldReasons> _vendorHoldReasons = null;
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.VendorHoldReasons>> GetAllVendorHoldReasonsAsync(bool bypassCache)
        {
            if (_vendorHoldReasons == null)
            {
                _vendorHoldReasons = await _colleagueFinanceReferenceDataRepository.GetVendorHoldReasonsAsync(bypassCache);
            }
            return _vendorHoldReasons;
        }

        private IEnumerable<Domain.ColleagueFinance.Entities.AccountsPayableSources> _accountsPayableSources = null;
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.AccountsPayableSources>> GetAllAccountsPayableSourcesAsync(bool bypassCache)
        {
            if (_accountsPayableSources == null)
            {
                _accountsPayableSources = await _colleagueFinanceReferenceDataRepository.GetAccountsPayableSourcesAsync(bypassCache);
            }
            return _accountsPayableSources;
        }

        private IEnumerable<Domain.ColleagueFinance.Entities.CurrencyConversion> _currencyConv = null;
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.CurrencyConversion>> GetAllCurrencyConversionAsync()
        {
            if (_currencyConv == null)
            {
                _currencyConv = await _colleagueFinanceReferenceDataRepository.GetCurrencyConversionAsync();
            }
            return _currencyConv;
        }
      
        #region EEDM vendor v8

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all vendors
        /// </summary>
        /// <returns>Collection of Vendors DTO objects</returns>
        public async Task<Tuple<IEnumerable<Vendors>, int>> GetVendorsAsync(int offset, int limit, VendorFilter criteriaValues, bool bypassCache = false)
        {
            CheckViewVendorPermission();

            var vendorsCollection = new List<Vendors>();

            var vendorIdCriteria = string.Empty;
            List<string> classificationCriteria = null;

            //check if vendorDetail criteria present and get the id to send in to the repo
            if (!string.IsNullOrEmpty(criteriaValues.vendorDetail))
            {
                try
                {
                    var personId = await _personRepository.GetPersonIdFromGuidAsync(criteriaValues.vendorDetail);
                    if (!string.IsNullOrEmpty(personId))
                    {
                        vendorIdCriteria = personId;
                    }
                    else
                    {
                        return new Tuple<IEnumerable<Vendors>, int>(new List<Vendors>(), 0);
                    }
                }
                catch
                {
                    return new Tuple<IEnumerable<Vendors>, int>(new List<Vendors>(), 0);
                }
            }

            //check if the classification criteria is present and get the vendor type code to send in to the repo
            if (!string.IsNullOrEmpty(criteriaValues.classifications))
            {
                
                var vendorTypesList = await GetAllVendorTypesAsync(bypassCache);
                var vendorType = vendorTypesList.Where(v => v.Guid == criteriaValues.classifications).FirstOrDefault();
                if (vendorType != null)
                {
                    classificationCriteria = new List<string>() { vendorType.Code };

                }
                else
                {
                    return new Tuple<IEnumerable<Vendors>, int>(new List<Vendors>(), 0);
                }
            }
            //Check if the relatedReference filter was sent in. If it was we will return a empty list as relatedReference is not
            //supported in vendors v8.
            if (criteriaValues.relatedReference != null)
            {
                if (criteriaValues.relatedReference.Any() && criteriaValues.relatedReference.Count > 0)
                {
                    return new Tuple<IEnumerable<Vendors>, int>(new List<Vendors>(), 0);
                }
            }
            if (criteriaValues.statuses != null)
            {
                if (criteriaValues.statuses.Any() && criteriaValues.statuses.Count > 0)
                {
                    foreach(var status in criteriaValues.statuses)
                    {
                        if(status.ToLower() != "active" && status.ToLower() != "holdpayment" && status.ToLower() != "approved")
                        {
                            return new Tuple<IEnumerable<Vendors>, int>(new List<Vendors>(), 0);
                        }
                    }
                    
                }
            }


            var vendorsEntities = await _vendorsRepository.GetVendorsAsync(offset, limit, vendorIdCriteria, classificationCriteria, criteriaValues.statuses);
            var totalRecords = vendorsEntities.Item2;

            if ((vendorsEntities != null) && (vendorsEntities.Item1.Any()))
            {
                var institutions = await _institutionRepository.GetInstitutionsFromListAsync(vendorsEntities.Item1.Select(x => x.Id).ToArray());

                foreach (var vendorsEntity in vendorsEntities.Item1)
                {
                    if (vendorsEntity.Guid != null)
                    {
                        var vendorDto = await ConvertVendorsEntityToDtoAsync(vendorsEntity, institutions);
                        vendorsCollection.Add(vendorDto);
                    }
                }
            }
            return new Tuple<IEnumerable<Vendors>, int>(vendorsCollection, totalRecords);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a Vendors from its GUID
        /// </summary>
        /// <returns>Vendors DTO object</returns>
        public async Task<Vendors> GetVendorsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a Vendor.");
            }
            CheckViewVendorPermission();

            try
            {
                var vendors = await _vendorsRepository.GetVendorsByGuidAsync(guid);
                List<Institution> institutions = null;
                if (vendors != null)
                {
                    institutions = (await _institutionRepository.GetInstitutionsFromListAsync(new string[] { vendors.Id })).ToList();
                }

                return await ConvertVendorsEntityToDtoAsync(vendors, institutions);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No vendor was found for guid  " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("No vendor was found for guid  " + guid, ex);
            }
            catch (RepositoryException ex)
            {
                throw new RepositoryException("No vendor was found for guid  " + guid, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("No vendor was found for guid  " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        ///  Update an existing Vendors domain entity from Dto
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="vendorDto">Vendors DTO</param>
        /// <returns>Vendors domain entity</returns>
        public async Task<Vendors> PutVendorAsync(string guid, Vendors vendorDto)
        {
            if (vendorDto == null)
                throw new ArgumentNullException("vendor", "Must provide a vendor for update");
            if (string.IsNullOrEmpty(vendorDto.Id))
                throw new ArgumentNullException("vendor", "Must provide a guid for vendor update");

            string vendorId;
            try
            {
                // get the  ID associated with the incoming guid
                vendorId = await _vendorsRepository.GetVendorIdFromGuidAsync(vendorDto.Id);
            } catch (KeyNotFoundException e)
            {
                vendorId = null;
            } catch (ArgumentException e)
            {
                throw new ArgumentException(e.Message);
            }
            

            // verify the GUID exists to perform an update.  If not, perform a create instead
            if (!string.IsNullOrEmpty(vendorId))
            {
                try
                {
                   
                    // verify the user has the permission to update a vendor
                    CheckUpdateVendorPermission();

                    _vendorsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                    var vendor = await _vendorsRepository.GetVendorsByGuidAsync(guid);
                    if (vendor != null)
                    {
                        if (vendorDto.StartOn != null && vendor.AddDate != null )
                        {
                            if (DateTime.Compare(Convert.ToDateTime(vendorDto.StartOn), Convert.ToDateTime(vendor.AddDate)) != 0)
                            {
                                throw new ArgumentException("startOn date can not be updated.");
                            }
                        }

                        //PUT - Updating to "holdPayment" (VEN.STOP.PAYMENT.FLAG = N) but a vendorHoldReason is not received.       
                        if (vendor.StopPaymentFlag == "Y")
                        {
                            if ((vendorDto.Statuses != null) && (!vendorDto.Statuses.Contains(VendorsStatuses.Holdpayment))                                )
                            {
                                throw new ArgumentNullException("Vendor.VendorHoldReasons", "The removal of the 'holdPayment' status for a vendor is not permitted.");
                            }
                           
                        }
                    }
                    // map the DTO to entities
                    var vendorEntity
                        = await ConvertVendorsDtoToEntityAsync(guid, vendorDto);

                    // update the entity in the database
                    var updatedVendorEntity =
                        await _vendorsRepository.UpdateVendorsAsync(vendorEntity);

                    List<Institution> institutions = null;
                    if (updatedVendorEntity != null)
                    {
                        institutions = (await _institutionRepository.GetInstitutionsFromListAsync(new string[] { updatedVendorEntity.Id })).ToList();
                    }
                    // return the newly updated DTO
                    return await ConvertVendorsEntityToDtoAsync(updatedVendorEntity, institutions, false);

                }
                catch (RepositoryException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message, ex.InnerException);
                }
            }
            // perform a create instead
            return await PostVendorAsync(vendorDto);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Create a new Vendors domain entity from Dto
        /// </summary>
        /// <param name="vendorDto">Vendors DTO</param>
        /// <returns>Vendors domain entity</returns>
        public async Task<Vendors> PostVendorAsync(Vendors vendorDto)
        {
            if (vendorDto == null)
                throw new ArgumentNullException("vendor", "Must provide a vendor for update");
            if (string.IsNullOrEmpty(vendorDto.Id))
                throw new ArgumentNullException("vendor", "Must provide a guid for vendor update");


            Domain.ColleagueFinance.Entities.Vendors createdVendor = null;

            // verify the user has the permission to create a Vendor
            CheckUpdateVendorPermission();

            _vendorsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            try
            {
                var vendorEntity
                         = await ConvertVendorsDtoToEntityAsync(vendorDto.Id, vendorDto);

                // create a Vendor entity in the database
                createdVendor =  await _vendorsRepository.CreateVendorsAsync(vendorEntity);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            List<Institution> institutions = null;
            if (createdVendor != null)
            {
                institutions = (await _institutionRepository.GetInstitutionsFromListAsync(new string[] { createdVendor.Id })).ToList();
            }
            // return the newly created Vendor
            return await ConvertVendorsEntityToDtoAsync(createdVendor, institutions, true);
     
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Convert Vendor Dto To Vendors Domain Entity
        /// </summary>
        /// <param name="vendorId">guid</param>
        /// <param name="vendorDto"><see cref="Dtos.Vendors">Vendor</see></param>
        /// <returns><see cref="Domain.ColleagueFinance.Entities.Vendors">Vendor</see></returns>
        private async Task<Domain.ColleagueFinance.Entities.Vendors> ConvertVendorsDtoToEntityAsync(string vendorId, Vendors vendorDto,
             bool bypassCache = true)
        {
            if (vendorDto == null || string.IsNullOrEmpty(vendorDto.Id))
                throw new ArgumentNullException("vendor", "Must provide guid for vendor");

            var vendorEntity = new Domain.ColleagueFinance.Entities.Vendors(vendorDto.Id);

            if (vendorDto.StartOn.HasValue)
            {
                vendorEntity.AddDate = vendorDto.StartOn;
            }

            if (vendorDto.DefaultCurrency != null)
            {
                var currencyCode = ConvertCurrencyIsoCodeToCurrencyCode(vendorDto.DefaultCurrency);
                var currencyCodes = (await GetAllCurrencyConversionAsync());
                if (currencyCodes == null)
                {
                    throw new KeyNotFoundException("Unable to extract currency codes from CURRENCY.CONV");
                }
                var curCode = currencyCodes.FirstOrDefault(x => x.CurrencyCode == currencyCode);
                if (curCode == null)
                {
                    throw new KeyNotFoundException(string.Concat("Unable to locate currency code: ", currencyCode));
                }
                vendorEntity.CurrencyCode = curCode.Code;
            }

            if (vendorDto.VendorDetail != null)
            {
                var vendorDetail = vendorDto.VendorDetail;
                if (vendorDetail.Institution != null && !(string.IsNullOrEmpty(vendorDetail.Institution.Id)))
                {
                    var person = await _personRepository.GetPersonByGuidNonCachedAsync(vendorDetail.Institution.Id);
                    if (person == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Unable to locate person record for guid: ", vendorDetail.Institution.Id));
                    }
                    if ( (string.IsNullOrEmpty(person.PersonCorpIndicator) || (person.PersonCorpIndicator == "N")))
                    {
                        throw new ArgumentException(string.Concat("The institution guid specified is a person: ", vendorDetail.Institution.Id));
                                 }

                    var institution =  await _institutionRepository.GetInstitutionsFromListAsync(new string[] { person.Id });
                    if (institution == null)
                    {
                        throw new ArgumentException(string.Concat("The institution specified is an organization.: ", vendorDetail.Institution.Id)); 
                    }
                    vendorEntity.Id = person.Id;
                }
                else if (vendorDetail.Organization != null && !(string.IsNullOrEmpty(vendorDetail.Organization.Id)))
                {
                    
                    var person = await _personRepository.GetPersonByGuidNonCachedAsync(vendorDetail.Organization.Id);
                    if (person == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Unable to locate person record for guid: ", vendorDetail.Organization.Id));
                    }

                    var institution = await _institutionRepository.GetInstitutionsFromListAsync(new string[] { person.Id });
                    if (institution != null && institution.Any())
                    {
                        throw new ArgumentException(string.Concat("The organization guid specified is an institution: ", vendorDetail.Organization.Id));
                    }
                    
                    if (person.PersonCorpIndicator != "Y")
                    {
                        throw new ArgumentException(string.Concat("The organization guid specified is a person: ", vendorDetail.Organization.Id));
                    }    
                    vendorEntity.Id = person.Id;

                    vendorEntity.IsOrganization = true;
                }
                else if (vendorDetail.Person != null && !(string.IsNullOrEmpty(vendorDetail.Person.Id)))
                {
                    var person = await _personRepository.GetPersonByGuidNonCachedAsync(vendorDetail.Person.Id);
                    if (person == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Unable to locate person record for guid: ", vendorDetail.Person.Id));
                    }
                    //var institution = (await GetInstitutions()).FirstOrDefault(i => i.Id.Equals(person.Id));
                    var institution = await _institutionRepository.GetInstitutionsFromListAsync(new string[] { person.Id });
                    if (institution != null && institution.Any())
                    {
                        throw new ArgumentException(string.Concat("The person guid specified is an institution: ", vendorDetail.Person.Id));
                    }
                    
                    if (person.PersonCorpIndicator == "Y")
                    {
                        throw new ArgumentException(string.Concat("The person specified is an organization.: ", vendorDetail.Person.Id));
                    }
                    vendorEntity.Id = person.Id;
                }
            }

            if (vendorDto.Statuses != null && vendorDto.Statuses.Any())
            {
                foreach (var status in vendorDto.Statuses)
                {
                    switch (status)
                    {
                        case VendorsStatuses.Active:
                            vendorEntity.ActiveFlag = "Y";
                            break;
                        case VendorsStatuses.Approved:
                            vendorEntity.ApprovalFlag = "Y";
                            break;
                        case VendorsStatuses.Holdpayment:
                            vendorEntity.StopPaymentFlag = "Y";
                            break;
                    }
                }
            }

            if (vendorDto.PaymentSources != null && vendorDto.PaymentSources.Any())
            {
                if (vendorDto.PaymentSources.Count() == 1 && vendorDto.PaymentSources[0].Id == null)
                {
                    throw new ArgumentException(" The paymentSources id is required when submitting paymentSources");
                }
                var sources = new List<string>();
                var accountsPayableSources = (await GetAllAccountsPayableSourcesAsync(bypassCache));
                if (accountsPayableSources == null)
                {
                    throw new KeyNotFoundException("Unable to extract accounts payable sources from AP.TYPES");
                }

                foreach (var source in vendorDto.PaymentSources)
                {
                    var accountsPayableSource = accountsPayableSources.FirstOrDefault(x => x.Guid == source.Id);
                    if (accountsPayableSource == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Unable to locate accounts payable source for guid: ", source.Id));
                    }
                    sources.Add(accountsPayableSource.Code);
                }

                if (sources.Any())
                {
                    vendorEntity.ApTypes = sources;
                }
            }

            if (vendorDto.PaymentTerms != null && vendorDto.PaymentTerms.Any())
            {
                var terms = new List<string>();
                var vendorTerms = (await GetAllVendorTermsAsync(bypassCache));
                if (vendorTerms == null)
                {
                    throw new KeyNotFoundException("Unable to extract vendor terms from VENDOR.TERMS");
                }
                foreach (var term in vendorDto.PaymentTerms)
                {
                    var paymentTerm = vendorTerms.FirstOrDefault((x => x.Guid == term.Id));
                    if (paymentTerm == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Unable to locate payment term for guid: ", term.Id));
                    }
                    terms.Add(paymentTerm.Code);
                }

                if (terms.Any())
                {
                    vendorEntity.Terms = terms;
                }
            }

            if (vendorDto.Classifications != null && vendorDto.Classifications.Any())
            {
                var types = new List<string>();
                var vendorTypes = (await GetAllVendorTypesAsync(bypassCache));
                if (vendorTypes == null)
                {
                    throw new KeyNotFoundException("Unable to extract vendor types from VENDOR.TYPES");
                }
                foreach (var classification in vendorDto.Classifications)
                {
                    var vendorType = vendorTypes.FirstOrDefault((x => x.Guid == classification.Id));
                    if (vendorType == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Unable to locate vendor type for guid: ", classification.Id));
                    }
                    types.Add(vendorType.Code);
                }

                if (types.Any())
                {
                    vendorEntity.Types = types;
                }
            }

            if (vendorDto.VendorHoldReasons != null && vendorDto.VendorHoldReasons.Any())
            {
                var vendorHoldReasons = new List<string>();
                var allVendorHoldReasons = await GetAllVendorHoldReasonsAsync(bypassCache);
                if (allVendorHoldReasons == null)
                {
                    throw new KeyNotFoundException("Unable to extract vendor hold reasons from INTG.VENDOR.HOLD.REASONS");
                }
                foreach (var holdReason in vendorDto.VendorHoldReasons)
                {
                    var vendorHoldReason = allVendorHoldReasons.FirstOrDefault(v => v.Guid == holdReason.Id);
                    if (vendorHoldReason == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Unable to locate vendor hold reason for guid: ", holdReason.Id));
                    }
                    vendorHoldReasons.Add(vendorHoldReason.Code);
                }
                if (vendorHoldReasons.Any())
                {
                    vendorEntity.IntgHoldReasons = vendorHoldReasons;
                }
            }
            if (!string.IsNullOrWhiteSpace(vendorDto.Comment))
            {
                vendorEntity.Comments = vendorDto.Comment;
            }

            return vendorEntity;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Vendors domain entity to its corresponding Vendors DTO
        /// </summary>
        /// <param name="source">Vendors domain entity</param>
        /// <returns>Vendors DTO</returns>
        private async Task<Vendors> ConvertVendorsEntityToDtoAsync(Domain.ColleagueFinance.Entities.Vendors source,
            IEnumerable<Domain.Base.Entities.Institution> institutions, bool bypassCache = false)
        {
            var vendors = new Vendors();

            if (!(string.IsNullOrEmpty(source.CurrencyCode)))
            {
                var currencyCodes = (await GetAllCurrencyConversionAsync()).ToList();

                var curCode = currencyCodes.FirstOrDefault(x => x.Code == source.CurrencyCode);
                if (curCode != null)
                {
                    var currencyIsoCode = ConvertCurrencyCodeToCurrencyIsoCode(curCode.CurrencyCode);
                    if (currencyIsoCode != null)
                    {
                        vendors.DefaultCurrency = currencyIsoCode;
                    }
                }
            }

            var personGuid = await _personRepository.GetPersonGuidFromIdAsync(source.Id);
            if (string.IsNullOrEmpty(personGuid))
            {
                throw new KeyNotFoundException(string.Concat("Unable to locate guid for PERSON id: ", source.Id));
            }
            
                var vendorDetail = new VendorDetailsDtoProperty();

            Domain.Base.Entities.Institution institution = null;
            if (institutions != null && institutions.Any())
                institution = institutions.FirstOrDefault(i => i.Id.Equals(source.Id));

            if (source.IsOrganization && institution == null)
                {
                    vendorDetail.Organization = new GuidObject2(personGuid);
                }
                else if (institution != null)
                {                    
                    vendorDetail.Institution = new GuidObject2(personGuid);
                }
                else
                {
                    vendorDetail.Person = new GuidObject2(personGuid);
                }
                vendors.VendorDetail = vendorDetail;
           

            if (((source.IsOrganization)||(institution != null)) 
                && (source.CorpParent != null) && (source.CorpParent.Any()))
            {
                var relatedVendors = new List<RelatedVendorDtoProperty>();

                foreach (var corpParent in source.CorpParent)
                {
                    //we only want parents that are also vendors
                    try
                    {
                        var corpParentGuid = await _vendorsRepository.GetVendorGuidFromIdAsync(corpParent);
                        if (!string.IsNullOrEmpty(corpParentGuid))
                        {
                            var relatedVendor = new RelatedVendorDtoProperty()
                            {
                                Type = VendorType.ParentVendor,
                                Vendor = new GuidObject2(corpParentGuid)
                            };
                            relatedVendors.Add(relatedVendor);
                        }
                    }
                    catch (Exception)
                    {
                        // do not throw error 
                    }
                                        
                }
                if (relatedVendors.Any())
                {
                    vendors.RelatedVendor = relatedVendors;
                }
            }

            var vendorsStatuses = new List<VendorsStatuses?>();
            
            if (source.ActiveFlag == "Y")
                vendorsStatuses.Add(VendorsStatuses.Active);
            if (source.StopPaymentFlag == "Y")
                vendorsStatuses.Add(VendorsStatuses.Holdpayment);
            if (source.ApprovalFlag == "Y")
                vendorsStatuses.Add(VendorsStatuses.Approved);

            if (vendorsStatuses.Any())
            {
                vendors.Statuses = vendorsStatuses;
            }

            if (source.IntgHoldReasons != null && source.IntgHoldReasons.Any())
            {
                var vendorHoldReasons = new List<GuidObject2>();
                var allVendorHoldReasons = await GetAllVendorHoldReasonsAsync(bypassCache);
                if (allVendorHoldReasons == null)
                    throw new KeyNotFoundException("Unable to extract vendor hold reasons from INTG.VENDOR.HOLD.REASONS");

                foreach (var holdReason in source.IntgHoldReasons)
                {
                    var vendorHoldReason = allVendorHoldReasons.FirstOrDefault(v => v.Code == holdReason);
                    if (vendorHoldReason == null)
                        throw new KeyNotFoundException("Unable to locate vendor hold reason for code: OB");
                    vendorHoldReasons.Add(new GuidObject2(vendorHoldReason.Guid));
                }
             
                if (vendorHoldReasons.Any())
                    vendors.VendorHoldReasons = vendorHoldReasons;
            }

            vendors.Id = source.Guid;
            vendors.StartOn = source.AddDate;

         
            if ((source.ApTypes != null) && (source.ApTypes.Any()))
            {
                var paymentSources = new List<GuidObject2>();

                var accountsPayableSources = (await GetAllAccountsPayableSourcesAsync(bypassCache)).ToList();
                if (!accountsPayableSources.Any())
                    throw new KeyNotFoundException("Unable to locate AccountsPayableSources");
                foreach (var apType in source.ApTypes)
                {
                    var accountsPayableSource = accountsPayableSources.FirstOrDefault((x => x.Code == apType));
                    if (accountsPayableSource != null)
                    {
                        paymentSources.Add(new GuidObject2(accountsPayableSource.Guid));
                    }
                }
                if (paymentSources.Any())
                {
                    vendors.PaymentSources = paymentSources;
                }
            }

            if ((source.Terms != null) && (source.Terms.Any()))
            {
                var paymentTerms = new List<GuidObject2>();

                var vendorTerms = (await GetAllVendorTermsAsync(bypassCache)).ToList();
                if (!vendorTerms.Any())
                    throw new KeyNotFoundException("Unable to locate VendorTerms");

                foreach (var term in source.Terms)
                {
                    var vendorTerm = vendorTerms.FirstOrDefault((x => x.Code == term));
                    if (vendorTerm != null)
                    {
                        paymentTerms.Add(new GuidObject2(vendorTerm.Guid));
                    }
                }
                if (paymentTerms.Any())
                {
                    vendors.PaymentTerms = paymentTerms;
                }
            }

            if ((source.Types != null) && (source.Types.Any()))
            {
                var classifications = new List<GuidObject2>();

                var vendorTypes = (await GetAllVendorTypesAsync(bypassCache)).ToList();
                if (!vendorTypes.Any())
                    throw new KeyNotFoundException("Unable to locate VendorTypes");

                foreach (var sourceType in source.Types)
                {
                    var vendorType = vendorTypes.FirstOrDefault((x => x.Code == sourceType));
                    if (vendorType != null)
                    {
                        classifications.Add(new GuidObject2(vendorType.Guid));
                    }
                }
                if (classifications.Any())
                {
                    vendors.Classifications = classifications;
                }
            }

            if (!string.IsNullOrWhiteSpace(source.Comments))
                vendors.Comment = source.Comments;

            return vendors;
        }

        #endregion

        #region EEDM vendor v11

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all vendors
        /// </summary>
        /// <returns>Collection of Vendors DTO objects</returns>
        public async Task<Tuple<IEnumerable<Vendors2>, int>> GetVendorsAsync2(int offset, int limit, string vendorDetails, List<string> classifications, 
            List<string> statuses,  List<string> relatedReferences, List<string> types = null, bool bypassCache = false)
        {
            CheckViewVendorPermission();

            var vendorsCollection = new List<Vendors2>();
           
            var vendorIdCriteria = string.Empty;
            List<string> classificationCriteria = null;

            //check if vendorDetail criteria present and get the id to send in to the repo
            if (!string.IsNullOrEmpty(vendorDetails))
            {
                try
                {
                    var personId = await _personRepository.GetPersonIdFromGuidAsync(vendorDetails);
                    if (!string.IsNullOrEmpty(personId))
                    {
                        vendorIdCriteria = personId;
                    }
                    else
                    {
                        return new Tuple<IEnumerable<Vendors2>, int>(new List<Vendors2>(), 0);
                    }
                }
                catch
                {
                    return new Tuple<IEnumerable<Vendors2>, int>(new List<Vendors2>(), 0);
                }
            }

            //check if the classification criteria is present and get the vendor type code to send in to the repo
            if (classifications != null && classifications.Any())
            {
                classificationCriteria = new List<string>();
                var vendorTypesList = await GetAllVendorTypesAsync(bypassCache);
                foreach (var classification in classifications)
                {
                    var vendorClassification = vendorTypesList.Where(v => v.Guid == classification).FirstOrDefault();
                    if (vendorClassification != null)
                    {
                        classificationCriteria.Add(vendorClassification.Code);
                    }
                    else
                    {
                        return new Tuple<IEnumerable<Vendors2>, int>(new List<Vendors2>(), 0);
                    }
                }
            }
            //Check if the relatedReference filter was sent in. If it was we will return a empty list as relatedReference is not
            //supported in vendors v8.
            if (relatedReferences  != null)
            {
                foreach (var relatedReference in relatedReferences)
                {
                    if (relatedReference.ToLower() == "paymentvendor")
                    {
                        return new Tuple<IEnumerable<Vendors2>, int>(new List<Vendors2>(), 0);
                    }
                }
            }

            if (statuses != null)
            {
                foreach (var status in statuses)
                {
                    if (status.ToLower() != "active" && status.ToLower() != "holdpayment" && status.ToLower() != "approved")
                    {
                        return new Tuple<IEnumerable<Vendors2>, int>(new List<Vendors2>(), 0);
                    }
                }
            }

            string[] venTypes = new []{ "eprocurement", "travel" };
            if (types != null && !types.Any())
            {
                return new Tuple<IEnumerable<Vendors2>, int>(new List<Vendors2>(), 0);                
            }

            if(types != null && types.Any())
            {
                foreach (var vendorType in types)
                {
                    if (!venTypes.Contains(vendorType.ToLower()))
                    {
                        return new Tuple<IEnumerable<Vendors2>, int>(new List<Vendors2>(), 0);
                    }
                }
            }


            var vendorsEntities = await _vendorsRepository.GetVendorsAsync(offset, limit, vendorIdCriteria, classificationCriteria, statuses, relatedReferences, types);
            var totalRecords = vendorsEntities.Item2;

            if ((vendorsEntities != null) && (vendorsEntities.Item1.Any()))
                {
                var institutions = await _institutionRepository.GetInstitutionsFromListAsync(vendorsEntities.Item1.Select(x => x.Id).ToArray());

                foreach (var vendorsEntity in vendorsEntities.Item1)
                {
                    if (vendorsEntity.Guid != null)
                    {
                        var vendorDto = await ConvertVendorsEntityToDtoAsync2(vendorsEntity, institutions, bypassCache);
                        vendorsCollection.Add(vendorDto);
                    }
                }
            }
            return new Tuple<IEnumerable<Vendors2>, int>(vendorsCollection, totalRecords);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a Vendors from its GUID
        /// </summary>
        /// <returns>Vendors DTO object</returns>
        public async Task<Vendors2> GetVendorsByGuidAsync2(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a Vendor.");
            }
            CheckViewVendorPermission();

            try
            {
                var vendors = await _vendorsRepository.GetVendorsByGuidAsync(guid);
                List<Institution> institutions = null;
                if (vendors != null)
                {
                    institutions = (await _institutionRepository.GetInstitutionsFromListAsync(new string[] { vendors.Id })).ToList();
                }

                return await ConvertVendorsEntityToDtoAsync2(vendors, institutions);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No vendor was found for guid  " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("No vendor was found for guid  " + guid, ex);
            }
            catch (RepositoryException ex)
            {
                throw new RepositoryException("No vendor was found for guid  " + guid, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("No vendor was found for guid  " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        ///  Update an existing Vendors domain entity from Dto
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="vendorDto">Vendors DTO</param>
        /// <returns>Vendors domain entity</returns>
        public async Task<Vendors2> PutVendorAsync2(string guid, Vendors2 vendorDto)
        {
            if (vendorDto == null)
                throw new ArgumentNullException("vendor", "Must provide a vendor for update");
            if (string.IsNullOrEmpty(vendorDto.Id))
                throw new ArgumentNullException("vendor", "Must provide a guid for vendor update");

            string vendorId;
            try
            {
                // get the  ID associated with the incoming guid
                vendorId = await _vendorsRepository.GetVendorIdFromGuidAsync(vendorDto.Id);
            }
            catch (KeyNotFoundException e)
            {
                vendorId = null;
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(e.Message);
            }


            // verify the GUID exists to perform an update.  If not, perform a create instead
            if (!string.IsNullOrEmpty(vendorId))
            {
                try
                {

                    // verify the user has the permission to update a vendor
                    CheckUpdateVendorPermission();

                    _vendorsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                    var vendor = await _vendorsRepository.GetVendorsByGuidAsync(guid);
                    if (vendor != null)
                    {
                        if (vendorDto.StartOn != null && vendor.AddDate != null)
                        {
                            if (DateTime.Compare(Convert.ToDateTime(vendorDto.StartOn), Convert.ToDateTime(vendor.AddDate)) != 0)
                            {
                                throw new ArgumentException("startOn date can not be updated.");
                            }
                        }

                        //PUT - Updating to "holdPayment" (VEN.STOP.PAYMENT.FLAG = N) but a vendorHoldReason is not received.       
                        if (vendor.StopPaymentFlag == "Y")
                        {
                            if ((vendorDto.Statuses != null) && (!vendorDto.Statuses.Contains(VendorsStatuses.Holdpayment)))
                            {
                                throw new ArgumentNullException("Vendor.VendorHoldReasons", "The removal of the 'holdPayment' status for a vendor is not permitted.");
                            }
                        }
                    }
                    // map the DTO to entities
                    var vendorEntity
                        = await ConvertVendorsDtoToEntityAsync2(guid, vendorDto);

                    // update the entity in the database
                    var updatedVendorEntity =
                        await _vendorsRepository.UpdateVendorsAsync(vendorEntity);

                    List<Institution> institutions = null;
                    if (updatedVendorEntity != null)
                    {
                        institutions = (await _institutionRepository.GetInstitutionsFromListAsync(new string[] { updatedVendorEntity.Id })).ToList();
                    }
                    // return the newly updated DTO
                    return await ConvertVendorsEntityToDtoAsync2(updatedVendorEntity, institutions, false);

                }
                catch (RepositoryException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message, ex.InnerException);
                }
            }
            // perform a create instead
            return await PostVendorAsync2(vendorDto);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Create a new Vendors domain entity from Dto
        /// </summary>
        /// <param name="vendorDto">Vendors DTO</param>
        /// <returns>Vendors domain entity</returns>
        public async Task<Vendors2> PostVendorAsync2(Vendors2 vendorDto)
        {
            if (vendorDto == null)
                throw new ArgumentNullException("vendor", "Must provide a vendor for update");
            if (string.IsNullOrEmpty(vendorDto.Id))
                throw new ArgumentNullException("vendor", "Must provide a guid for vendor update");


            Domain.ColleagueFinance.Entities.Vendors createdVendor = null;

            // verify the user has the permission to create a Vendor
            CheckUpdateVendorPermission();

            _vendorsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            try
            {
                var vendorEntity
                         = await ConvertVendorsDtoToEntityAsync2(vendorDto.Id, vendorDto);

                // create a Vendor entity in the database
                createdVendor = await _vendorsRepository.CreateVendorsAsync(vendorEntity);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            List<Institution> institutions = null;
            if (createdVendor != null)
            {
                institutions = (await _institutionRepository.GetInstitutionsFromListAsync(new string[] { createdVendor.Id })).ToList();
            }

            // return the newly created Vendor
            return await ConvertVendorsEntityToDtoAsync2(createdVendor, institutions, true);

        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Convert Vendor Dto To Vendors Domain Entity
        /// </summary>
        /// <param name="vendorId">guid</param>
        /// <param name="vendorDto"><see cref="Dtos.Vendors">Vendor</see></param>
        /// <returns><see cref="Domain.ColleagueFinance.Entities.Vendors">Vendor</see></returns>
        private async Task<Domain.ColleagueFinance.Entities.Vendors> ConvertVendorsDtoToEntityAsync2(string vendorId, 
            Vendors2 vendorDto, bool bypassCache = true)
        {
            if (vendorDto == null || string.IsNullOrEmpty(vendorDto.Id))
                throw new ArgumentNullException("vendor", "Must provide guid for vendor");

            var vendorEntity = new Domain.ColleagueFinance.Entities.Vendors(vendorDto.Id);

            if (vendorDto.StartOn.HasValue)
            {
                vendorEntity.AddDate = vendorDto.StartOn;
            }

            if (vendorDto.DefaultCurrency != null)
            {
                var currencyCode = ConvertCurrencyIsoCodeToCurrencyCode(vendorDto.DefaultCurrency);
                var currencyCodes = (await GetAllCurrencyConversionAsync());
                if (currencyCodes == null)
                {
                    throw new KeyNotFoundException("Unable to extract currency codes from CURRENCY.CONV");
                }
                var curCode = currencyCodes.FirstOrDefault(x => x.CurrencyCode == currencyCode);
                if (curCode == null)
                {
                    throw new KeyNotFoundException(string.Concat("Unable to locate currency code: ", currencyCode));
                }
                vendorEntity.CurrencyCode = curCode.Code;
            }

            if (vendorDto.VendorDetail != null)
            {
                var vendorDetail = vendorDto.VendorDetail;
                if (vendorDetail.Institution != null && !(string.IsNullOrEmpty(vendorDetail.Institution.Id)))
                {
                    var person = await _personRepository.GetPersonByGuidNonCachedAsync(vendorDetail.Institution.Id);
                    if (person == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Unable to locate person record for guid: ", vendorDetail.Institution.Id));
                    }
                    if ((string.IsNullOrEmpty(person.PersonCorpIndicator) || (person.PersonCorpIndicator == "N")))
                    {
                        throw new ArgumentException(string.Concat("The institution guid specified is a person: ", vendorDetail.Institution.Id));
                    }
                    var institution = await _institutionRepository.GetInstitutionsFromListAsync(new string[] { person.Id });
                    if (institution == null)
                    {
                        throw new ArgumentException(string.Concat("The institution specified is an organization.: ", vendorDetail.Institution.Id));
                    }
                    vendorEntity.Id = person.Id;
                }
                else if (vendorDetail.Organization != null && !(string.IsNullOrEmpty(vendorDetail.Organization.Id)))
                {

                    var person = await _personRepository.GetPersonByGuidNonCachedAsync(vendorDetail.Organization.Id);
                    if (person == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Unable to locate person record for guid: ", vendorDetail.Organization.Id));
                    }
                    var institution = await _institutionRepository.GetInstitutionsFromListAsync(new string[] { person.Id });
                    if (institution != null && institution.Any())
                    {
                        throw new ArgumentException(string.Concat("The organization guid specified is an institution: ", vendorDetail.Organization.Id));
                    }
                    if (person.PersonCorpIndicator != "Y")
                    {
                        throw new ArgumentException(string.Concat("The organization guid specified is a person: ", vendorDetail.Organization.Id));
                    }
                    vendorEntity.Id = person.Id;

                    vendorEntity.IsOrganization = true;
                }
                else if (vendorDetail.Person != null && !(string.IsNullOrEmpty(vendorDetail.Person.Id)))
                {
                    var person = await _personRepository.GetPersonByGuidNonCachedAsync(vendorDetail.Person.Id);
                    if (person == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Unable to locate person record for guid: ", vendorDetail.Person.Id));
                    }
                    var institution = await _institutionRepository.GetInstitutionsFromListAsync(new string[] { person.Id });
                    if (institution != null && institution.Any())
                    {
                        throw new ArgumentException(string.Concat("The person guid specified is an institution: ", vendorDetail.Person.Id));
                    }

                    if (person.PersonCorpIndicator == "Y")
                    {
                        throw new ArgumentException(string.Concat("The person specified is an organization.: ", vendorDetail.Person.Id));
                    }
                    vendorEntity.Id = person.Id;
                }
            }

            if (vendorDto.Statuses != null && vendorDto.Statuses.Any())
            {
                foreach (var status in vendorDto.Statuses)
                {
                    switch (status)
                    {
                        case VendorsStatuses.Active:
                            vendorEntity.ActiveFlag = "Y";
                            break;
                        case VendorsStatuses.Approved:
                            vendorEntity.ApprovalFlag = "Y";
                            break;
                        case VendorsStatuses.Holdpayment:
                            vendorEntity.StopPaymentFlag = "Y";
                            break;
                    }
                }
            }

            if (vendorDto.PaymentSources != null && vendorDto.PaymentSources.Any())
            {
                if (vendorDto.PaymentSources.Count() == 1 && vendorDto.PaymentSources[0].Id == null)
                {
                    throw new ArgumentException(" The paymentSources id is required when submitting paymentSources");
                }
                var sources = new List<string>();
                var accountsPayableSources = (await GetAllAccountsPayableSourcesAsync(bypassCache));
                if (accountsPayableSources == null)
                {
                    throw new KeyNotFoundException("Unable to extract accounts payable sources from AP.TYPES");
                }

                foreach (var source in vendorDto.PaymentSources)
                {
                    var accountsPayableSource = accountsPayableSources.FirstOrDefault(x => x.Guid == source.Id);
                    if (accountsPayableSource == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Unable to locate accounts payable source for guid: ", source.Id));
                    }
                    sources.Add(accountsPayableSource.Code);
                }

                if (sources.Any())
                {
                    vendorEntity.ApTypes = sources;
                }
            }

            if (vendorDto.PaymentTerms != null && vendorDto.PaymentTerms.Any())
            {
                var terms = new List<string>();
                var vendorTerms = (await GetAllVendorTermsAsync(bypassCache));
                if (vendorTerms == null)
                {
                    throw new KeyNotFoundException("Unable to extract vendor terms from VENDOR.TERMS");
                }
                foreach (var term in vendorDto.PaymentTerms)
                {
                    var paymentTerm = vendorTerms.FirstOrDefault((x => x.Guid == term.Id));
                    if (paymentTerm == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Unable to locate payment term for guid: ", term.Id));
                    }
                    terms.Add(paymentTerm.Code);
                }

                if (terms.Any())
                {
                    vendorEntity.Terms = terms;
                }
            }

            if (vendorDto.Classifications != null && vendorDto.Classifications.Any())
            {
                var types = new List<string>();
                var vendorTypes = (await GetAllVendorTypesAsync(bypassCache));
                if (vendorTypes == null)
                {
                    throw new KeyNotFoundException("Unable to extract vendor types from VENDOR.TYPES");
                }
                foreach (var classification in vendorDto.Classifications)
                {
                    var vendorType = vendorTypes.FirstOrDefault((x => x.Guid == classification.Id));
                    if (vendorType == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Unable to locate vendor type for guid: ", classification.Id));
                    }
                    types.Add(vendorType.Code);
                }

                if (types.Any())
                {
                    vendorEntity.Types = types;
                }
            }

            if (vendorDto.VendorHoldReasons != null && vendorDto.VendorHoldReasons.Any())
            {
                var vendorHoldReasons = new List<string>();
                var allVendorHoldReasons = await GetAllVendorHoldReasonsAsync(bypassCache);
                if (allVendorHoldReasons == null)
                {
                    throw new KeyNotFoundException("Unable to extract vendor hold reasons from INTG.VENDOR.HOLD.REASONS");
                }
                foreach (var holdReason in vendorDto.VendorHoldReasons)
                {
                    var vendorHoldReason = allVendorHoldReasons.FirstOrDefault(v => v.Guid == holdReason.Id);
                    if (vendorHoldReason == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Unable to locate vendor hold reason for guid: ", holdReason.Id));
                    }
                    vendorHoldReasons.Add(vendorHoldReason.Code);
                }
                if (vendorHoldReasons.Any())
                {
                    vendorEntity.IntgHoldReasons = vendorHoldReasons;
                }
            }
            if (!string.IsNullOrWhiteSpace(vendorDto.Comment))
            {
                vendorEntity.Comments = vendorDto.Comment;
            }

            vendorEntity.Categories = new List<string>();

            if (vendorDto.Types != null && vendorDto.Types.Any())
            {
                foreach(var cat in vendorDto.Types)
                {
                    switch (cat)
                    {
                        case VendorTypes.EProcurement:
                            vendorEntity.Categories.Add("eProcurement");
                            break;
                        case VendorTypes.Travel:
                            vendorEntity.Categories.Add("travel");
                            break;
                    }
                }
            }
            
            return vendorEntity;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Vendors domain entity to its corresponding Vendors DTO
        /// </summary>
        /// <param name="source">Vendors domain entity</param>
        /// <returns>Vendors DTO</returns>
        private async Task<Vendors2> ConvertVendorsEntityToDtoAsync2(Domain.ColleagueFinance.Entities.Vendors source,
             IEnumerable<Domain.Base.Entities.Institution> institutions, bool bypassCache = false)
        {
            var vendors = new Vendors2();

            if (!(string.IsNullOrEmpty(source.CurrencyCode)))
            {
                var currencyCodes = (await GetAllCurrencyConversionAsync()).ToList();

                var curCode = currencyCodes.FirstOrDefault(x => x.Code == source.CurrencyCode);
                if (curCode != null)
                {
                    var currencyIsoCode = ConvertCurrencyCodeToCurrencyIsoCode(curCode.CurrencyCode);
                    if (currencyIsoCode != null)
                    {
                        vendors.DefaultCurrency = currencyIsoCode;
                    }
                }
            }

            var personGuid = await _personRepository.GetPersonGuidFromIdAsync(source.Id);
            if (string.IsNullOrEmpty(personGuid))
            {
                throw new KeyNotFoundException(string.Concat("Unable to locate guid for PERSON id: ", source.Id));
            }

            var vendorDetail = new VendorDetailsDtoProperty();

            //var institution = (await GetInstitutions()).FirstOrDefault(i => i.Id.Equals(source.Id));
            Domain.Base.Entities.Institution institution = null;
            if ( institutions != null && institutions.Any())
                institution = institutions.FirstOrDefault(i => i.Id.Equals(source.Id));

            if (source.IsOrganization && institution == null)
            {
                vendorDetail.Organization = new GuidObject2(personGuid);
            }
            else if (institution != null)
            {
                vendorDetail.Institution = new GuidObject2(personGuid);
            }
            else
            {
                vendorDetail.Person = new GuidObject2(personGuid);
            }
            vendors.VendorDetail = vendorDetail;
            
            if (((source.IsOrganization) || (institution != null))
                && (source.CorpParent != null) && (source.CorpParent.Any()))
            {
                var relatedVendors = new List<RelatedVendorDtoProperty>();

                foreach (var corpParent in source.CorpParent)
                {
                    //we only want parents that are also vendors
                    try
                    {
                        var corpParentGuid = await _vendorsRepository.GetVendorGuidFromIdAsync(corpParent);
                        if (!string.IsNullOrEmpty(corpParentGuid))
                        {
                            var relatedVendor = new RelatedVendorDtoProperty()
                            {
                                Type = VendorType.ParentVendor,
                                Vendor = new GuidObject2(corpParentGuid)
                            };
                            relatedVendors.Add(relatedVendor);
                        }
                    }
                    catch (Exception)
                    {
                        // do not throw error 
                    }

                }
                if (relatedVendors.Any())
                {
                    vendors.RelatedVendor = relatedVendors;
                }
            }

            var vendorsStatuses = new List<VendorsStatuses?>();

            if (source.ActiveFlag == "Y")
                vendorsStatuses.Add(VendorsStatuses.Active);
            if (source.StopPaymentFlag == "Y")
                vendorsStatuses.Add(VendorsStatuses.Holdpayment);
            if (source.ApprovalFlag == "Y")
                vendorsStatuses.Add(VendorsStatuses.Approved);

            if (vendorsStatuses.Any())
            {
                vendors.Statuses = vendorsStatuses;
            }

            if (source.IntgHoldReasons != null && source.IntgHoldReasons.Any())
            {
                var vendorHoldReasons = new List<GuidObject2>();
                var allVendorHoldReasons = await GetAllVendorHoldReasonsAsync(bypassCache);
                if (allVendorHoldReasons == null)
                    throw new KeyNotFoundException("Unable to extract vendor hold reasons from INTG.VENDOR.HOLD.REASONS");

                foreach (var holdReason in source.IntgHoldReasons)
                {
                    var vendorHoldReason = allVendorHoldReasons.FirstOrDefault(v => v.Code == holdReason);
                    if (vendorHoldReason == null)
                        throw new KeyNotFoundException("Unable to locate vendor hold reason for code: OB");
                    vendorHoldReasons.Add(new GuidObject2(vendorHoldReason.Guid));
                }

                if (vendorHoldReasons.Any())
                    vendors.VendorHoldReasons = vendorHoldReasons;
            }

            vendors.Id = source.Guid;
            vendors.StartOn = source.AddDate;


            if ((source.ApTypes != null) && (source.ApTypes.Any()))
            {
                var paymentSources = new List<GuidObject2>();

                var accountsPayableSources = (await GetAllAccountsPayableSourcesAsync(bypassCache)).ToList();
                if (!accountsPayableSources.Any())
                    throw new KeyNotFoundException("Unable to locate AccountsPayableSources");
                foreach (var apType in source.ApTypes)
                {
                    var accountsPayableSource = accountsPayableSources.FirstOrDefault((x => x.Code == apType));
                    if (accountsPayableSource != null)
                    {
                        paymentSources.Add(new GuidObject2(accountsPayableSource.Guid));
                    }
                }
                if (paymentSources.Any())
                {
                    vendors.PaymentSources = paymentSources;
                }
            }

            if ((source.Terms != null) && (source.Terms.Any()))
            {
                var paymentTerms = new List<GuidObject2>();

                var vendorTerms = (await GetAllVendorTermsAsync(bypassCache)).ToList();
                if (!vendorTerms.Any())
                    throw new KeyNotFoundException("Unable to locate VendorTerms");

                foreach (var term in source.Terms)
                {
                    var vendorTerm = vendorTerms.FirstOrDefault((x => x.Code == term));
                    if (vendorTerm != null)
                    {
                        paymentTerms.Add(new GuidObject2(vendorTerm.Guid));
                    }
                }
                if (paymentTerms.Any())
                {
                    vendors.PaymentTerms = paymentTerms;
                }
            }

            if ((source.Types != null) && (source.Types.Any()))
            {
                var classifications = new List<GuidObject2>();

                var vendorTypes = (await GetAllVendorTypesAsync(bypassCache)).ToList();
                if (!vendorTypes.Any())
                    throw new KeyNotFoundException("Unable to locate VendorTypes");

                foreach (var sourceType in source.Types)
                {
                    var vendorType = vendorTypes.FirstOrDefault((x => x.Code == sourceType));
                    if (vendorType != null)
                    {
                        classifications.Add(new GuidObject2(vendorType.Guid));
                    }
                }
                if (classifications.Any())
                {
                    vendors.Classifications = classifications;
                }
            }

            if (!string.IsNullOrWhiteSpace(source.Comments))
                vendors.Comment = source.Comments;

            if (source.Categories != null)
            {
                if (source.Categories.Any() && source.Categories.Count > 0)
                {
                    vendors.Types = new List<VendorTypes>();
                    foreach (var category in source.Categories)
                    {
                        switch (category)
                        {
                            case "EP":
                                vendors.Types.Add(VendorTypes.EProcurement);
                                break;
                            case "TR":
                                vendors.Types.Add(VendorTypes.Travel);
                                break;
                        }
                    }
                }
            }
            return vendors;
        }

        #endregion

        /// <summary>
        /// Helper method to determine if the user has permission to view data.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewVendorPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.ViewVendors);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view Vendors.");
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to create/update vendors.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckUpdateVendorPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.UpdateVendors);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to create/update Vendors.");
            }
        }

        /// <summary>
        /// Convert CurrencyCodes domain enumeration to CurrencyIsoCode DTO enumeration
        /// </summary>
        /// <param name="code">CurrencyCodes domain enumeration</param>
        /// <returns>CurrencyIsoCode DTO enumeration</returns>
        private CurrencyIsoCode? ConvertCurrencyCodeToCurrencyIsoCode(Domain.ColleagueFinance.Entities.CurrencyCodes? code)
        {
            if (code == null)
                return null;

            switch (code)
            {
                case Domain.ColleagueFinance.Entities.CurrencyCodes.CAD:
                    return CurrencyIsoCode.CAD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.EUR:
                    return CurrencyIsoCode.EUR;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.USD:
                    return CurrencyIsoCode.USD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.AED:
                    return CurrencyIsoCode.AED;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.AFN:
                    return CurrencyIsoCode.AFN;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.ALL:
                    return CurrencyIsoCode.ALL;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.AMD:
                    return CurrencyIsoCode.AMD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.ANG:
                    return CurrencyIsoCode.ANG;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.AOA:
                    return CurrencyIsoCode.AOA;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.ARS:
                    return CurrencyIsoCode.ARS;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.AUD:
                    return CurrencyIsoCode.AUD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.AWG:
                    return CurrencyIsoCode.AWG;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.AZN:
                    return CurrencyIsoCode.AZN;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.BAM:
                    return CurrencyIsoCode.BAM;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.BBD:
                    return CurrencyIsoCode.BBD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.BDT:
                    return CurrencyIsoCode.BDT;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.BGN:
                    return CurrencyIsoCode.BGN;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.BHD:
                    return CurrencyIsoCode.BHD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.BIF:
                    return CurrencyIsoCode.BIF;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.BMD:
                    return CurrencyIsoCode.BMD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.BND:
                    return CurrencyIsoCode.BND;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.BOB:
                    return CurrencyIsoCode.BOB;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.BRL:
                    return CurrencyIsoCode.BRL;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.BSD:
                    return CurrencyIsoCode.BSD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.BTN:
                    return CurrencyIsoCode.BTN;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.BWP:
                    return CurrencyIsoCode.BWP;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.BYR:
                    return CurrencyIsoCode.BYR;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.BZD:
                    return CurrencyIsoCode.BZD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.CDF:
                    return CurrencyIsoCode.CDF;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.CHF:
                    return CurrencyIsoCode.CHF;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.CLP:
                    return CurrencyIsoCode.CLP;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.CNY:
                    return CurrencyIsoCode.CNY;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.COP:
                    return CurrencyIsoCode.COP;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.CRC:
                    return CurrencyIsoCode.CRC;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.CUC:
                    return CurrencyIsoCode.CUC;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.CUP:
                    return CurrencyIsoCode.CUP;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.CVE:
                    return CurrencyIsoCode.CVE;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.CZK:
                    return CurrencyIsoCode.CZK;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.DJF:
                    return CurrencyIsoCode.DJF;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.DKK:
                    return CurrencyIsoCode.DKK;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.DOP:
                    return CurrencyIsoCode.DOP;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.DZD:
                    return CurrencyIsoCode.DZD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.EGP:
                    return CurrencyIsoCode.EGP;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.ERN:
                    return CurrencyIsoCode.ERN;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.ETB:
                    return CurrencyIsoCode.ETB;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.FJD:
                    return CurrencyIsoCode.FJD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.FKP:
                    return CurrencyIsoCode.FKP;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.GBP:
                    return CurrencyIsoCode.GBP;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.GEL:
                    return CurrencyIsoCode.GEL;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.GHS:
                    return CurrencyIsoCode.GHS;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.GIP:
                    return CurrencyIsoCode.GIP;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.GMD:
                    return CurrencyIsoCode.GMD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.GNF:
                    return CurrencyIsoCode.GNF;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.GTQ:
                    return CurrencyIsoCode.GTQ;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.GYD:
                    return CurrencyIsoCode.GYD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.HKD:
                    return CurrencyIsoCode.HKD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.HNL:
                    return CurrencyIsoCode.HNL;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.HRK:
                    return CurrencyIsoCode.HRK;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.HTG:
                    return CurrencyIsoCode.HTG;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.HUF:
                    return CurrencyIsoCode.HUF;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.IDR:
                    return CurrencyIsoCode.IDR;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.ILS:
                    return CurrencyIsoCode.ILS;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.INR:
                    return CurrencyIsoCode.INR;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.IQD:
                    return CurrencyIsoCode.IQD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.IRR:
                    return CurrencyIsoCode.IRR;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.ISK:
                    return CurrencyIsoCode.ISK;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.JMD:
                    return CurrencyIsoCode.JMD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.JOD:
                    return CurrencyIsoCode.JOD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.JPY:
                    return CurrencyIsoCode.JPY;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.KES:
                    return CurrencyIsoCode.KES;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.KGS:
                    return CurrencyIsoCode.KGS;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.KHR:
                    return CurrencyIsoCode.KHR;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.KMF:
                    return CurrencyIsoCode.KMF;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.KPW:
                    return CurrencyIsoCode.KPW;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.KRW:
                    return CurrencyIsoCode.KRW;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.KWD:
                    return CurrencyIsoCode.KWD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.KYD:
                    return CurrencyIsoCode.KYD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.KZT:
                    return CurrencyIsoCode.KZT;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.LAK:
                    return CurrencyIsoCode.LAK;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.LBP:
                    return CurrencyIsoCode.LBP;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.LKR:
                    return CurrencyIsoCode.LKR;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.LRD:
                    return CurrencyIsoCode.LRD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.LSL:
                    return CurrencyIsoCode.LSL;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.LYD:
                    return CurrencyIsoCode.LYD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.MAD:
                    return CurrencyIsoCode.MAD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.MDL:
                    return CurrencyIsoCode.MDL;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.MGA:
                    return CurrencyIsoCode.MGA;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.MKD:
                    return CurrencyIsoCode.MKD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.MMK:
                    return CurrencyIsoCode.MMK;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.MNT:
                    return CurrencyIsoCode.MNT;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.MOP:
                    return CurrencyIsoCode.MOP;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.MRO:
                    return CurrencyIsoCode.MRO;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.MUR:
                    return CurrencyIsoCode.MUR;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.MVR:
                    return CurrencyIsoCode.MVR;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.MWK:
                    return CurrencyIsoCode.MWK;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.MXN:
                    return CurrencyIsoCode.MXN;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.MYR:
                    return CurrencyIsoCode.MYR;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.MZN:
                    return CurrencyIsoCode.MZN;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.NAD:
                    return CurrencyIsoCode.NAD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.NGN:
                    return CurrencyIsoCode.NGN;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.NIO:
                    return CurrencyIsoCode.NIO;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.NOK:
                    return CurrencyIsoCode.NOK;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.NPR:
                    return CurrencyIsoCode.NPR;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.NZD:
                    return CurrencyIsoCode.NZD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.OMR:
                    return CurrencyIsoCode.OMR;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.PAB:
                    return CurrencyIsoCode.PAB;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.PEN:
                    return CurrencyIsoCode.PEN;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.PGK:
                    return CurrencyIsoCode.PGK;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.PHP:
                    return CurrencyIsoCode.PHP;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.PKR:
                    return CurrencyIsoCode.PKR;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.PLN:
                    return CurrencyIsoCode.PLN;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.PYG:
                    return CurrencyIsoCode.PYG;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.QAR:
                    return CurrencyIsoCode.QAR;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.RON:
                    return CurrencyIsoCode.RON;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.RSD:
                    return CurrencyIsoCode.RSD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.RUB:
                    return CurrencyIsoCode.RUB;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.RWF:
                    return CurrencyIsoCode.RWF;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.SAR:
                    return CurrencyIsoCode.SAR;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.SBD:
                    return CurrencyIsoCode.SBD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.SCR:
                    return CurrencyIsoCode.SCR;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.SDG:
                    return CurrencyIsoCode.SDG;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.SEK:
                    return CurrencyIsoCode.SEK;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.SGD:
                    return CurrencyIsoCode.SGD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.SHP:
                    return CurrencyIsoCode.SHP;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.SLL:
                    return CurrencyIsoCode.SLL;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.SOS:
                    return CurrencyIsoCode.SOS;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.SRD:
                    return CurrencyIsoCode.SRD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.SSP:
                    return CurrencyIsoCode.SSP;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.STD:
                    return CurrencyIsoCode.STD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.SVC:
                    return CurrencyIsoCode.SVC;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.SYP:
                    return CurrencyIsoCode.SYP;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.SZL:
                    return CurrencyIsoCode.SZL;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.THB:
                    return CurrencyIsoCode.THB;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.TJS:
                    return CurrencyIsoCode.TJS;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.TMT:
                    return CurrencyIsoCode.TMT;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.TND:
                    return CurrencyIsoCode.TND;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.TOP:
                    return CurrencyIsoCode.TOP;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.TRY:
                    return CurrencyIsoCode.TRY;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.TTD:
                    return CurrencyIsoCode.TTD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.TWD:
                    return CurrencyIsoCode.TWD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.TZS:
                    return CurrencyIsoCode.TZS;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.UAH:
                    return CurrencyIsoCode.UAH;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.UGX:
                    return CurrencyIsoCode.UGX;

                case Domain.ColleagueFinance.Entities.CurrencyCodes.UYU:
                    return CurrencyIsoCode.UYU;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.UZS:
                    return CurrencyIsoCode.UZS;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.VEF:
                    return CurrencyIsoCode.VEF;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.VND:
                    return CurrencyIsoCode.VND;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.VUV:
                    return CurrencyIsoCode.VUV;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.WST:
                    return CurrencyIsoCode.WST;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.XAF:
                    return CurrencyIsoCode.XAF;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.XCD:
                    return CurrencyIsoCode.XCD;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.XOF:
                    return CurrencyIsoCode.XOF;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.XPF:
                    return CurrencyIsoCode.XPF;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.YER:
                    return CurrencyIsoCode.YER;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.ZAR:
                    return CurrencyIsoCode.ZAR;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.ZMW:
                    return CurrencyIsoCode.ZMW;
                case Domain.ColleagueFinance.Entities.CurrencyCodes.ZWL:
                    return CurrencyIsoCode.ZWL;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Convert CurrencyIsoCode domain enumeration to CurrencyCode DTO enumeration
        /// </summary>
        /// <param name="code">CurrencyIsoCode domain enumeration</param>
        /// <returns>CurrencyCode DTO enumeration</returns>
        private Domain.ColleagueFinance.Entities.CurrencyCodes? ConvertCurrencyIsoCodeToCurrencyCode(CurrencyIsoCode? code)
        {
            if (code == null)
                return null;

            switch (code)
            {
                case CurrencyIsoCode.CAD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.CAD;
                case CurrencyIsoCode.EUR:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.EUR;
                case CurrencyIsoCode.USD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.USD;
                case CurrencyIsoCode.AED:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.AED;
                case CurrencyIsoCode.AFN:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.AFN;
                case CurrencyIsoCode.ALL:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.ALL;
                case CurrencyIsoCode.AMD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.AMD;
                case CurrencyIsoCode.ANG:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.ANG;
                case CurrencyIsoCode.AOA:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.AOA;
                case CurrencyIsoCode.ARS:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.ARS;
                case CurrencyIsoCode.AUD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.AUD;
                case CurrencyIsoCode.AWG:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.AWG;
                case CurrencyIsoCode.AZN:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.AZN;
                case CurrencyIsoCode.BAM:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.BAM;
                case CurrencyIsoCode.BBD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.BBD;
                case CurrencyIsoCode.BDT:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.BDT;
                case CurrencyIsoCode.BGN:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.BGN;
                case CurrencyIsoCode.BHD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.BHD;
                case CurrencyIsoCode.BIF:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.BIF;
                case CurrencyIsoCode.BMD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.BMD;
                case CurrencyIsoCode.BND:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.BND;
                case CurrencyIsoCode.BOB:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.BOB;
                case CurrencyIsoCode.BRL:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.BRL;
                case CurrencyIsoCode.BSD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.BSD;
                case CurrencyIsoCode.BTN:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.BTN;
                case CurrencyIsoCode.BWP:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.BWP;
                case CurrencyIsoCode.BYR:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.BYR;
                case CurrencyIsoCode.BZD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.BZD;
                case CurrencyIsoCode.CDF:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.CDF;
                case CurrencyIsoCode.CHF:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.CHF;
                case CurrencyIsoCode.CLP:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.CLP;
                case CurrencyIsoCode.CNY:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.CNY;
                case CurrencyIsoCode.COP:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.COP;
                case CurrencyIsoCode.CRC:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.CRC;
                case CurrencyIsoCode.CUC:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.CUC;
                case CurrencyIsoCode.CUP:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.CUP;
                case CurrencyIsoCode.CVE:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.CVE;
                case CurrencyIsoCode.CZK:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.CZK;
                case CurrencyIsoCode.DJF:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.DJF;
                case CurrencyIsoCode.DKK:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.DKK;
                case CurrencyIsoCode.DOP:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.DOP;
                case CurrencyIsoCode.DZD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.DZD;
                case CurrencyIsoCode.EGP:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.EGP;
                case CurrencyIsoCode.ERN:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.ERN;
                case CurrencyIsoCode.ETB:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.ETB;
                case CurrencyIsoCode.FJD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.FJD;
                case CurrencyIsoCode.FKP:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.FKP;
                case CurrencyIsoCode.GBP:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.GBP;
                case CurrencyIsoCode.GEL:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.GEL;
                case CurrencyIsoCode.GHS:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.GHS;
                case CurrencyIsoCode.GIP:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.GIP;
                case CurrencyIsoCode.GMD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.GMD;
                case CurrencyIsoCode.GNF:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.GNF;
                case CurrencyIsoCode.GTQ:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.GTQ;
                case CurrencyIsoCode.GYD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.GYD;
                case CurrencyIsoCode.HKD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.HKD;
                case CurrencyIsoCode.HNL:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.HNL;
                case CurrencyIsoCode.HRK:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.HRK;
                case CurrencyIsoCode.HTG:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.HTG;
                case CurrencyIsoCode.HUF:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.HUF;
                case CurrencyIsoCode.IDR:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.IDR;
                case CurrencyIsoCode.ILS:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.ILS;
                case CurrencyIsoCode.INR:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.INR;
                case CurrencyIsoCode.IQD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.IQD;
                case CurrencyIsoCode.IRR:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.IRR;
                case CurrencyIsoCode.ISK:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.ISK;
                case CurrencyIsoCode.JMD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.JMD;
                case CurrencyIsoCode.JOD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.JOD;
                case CurrencyIsoCode.JPY:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.JPY;
                case CurrencyIsoCode.KES:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.KES;
                case CurrencyIsoCode.KGS:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.KGS;
                case CurrencyIsoCode.KHR:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.KHR;
                case CurrencyIsoCode.KMF:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.KMF;
                case CurrencyIsoCode.KPW:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.KPW;
                case CurrencyIsoCode.KRW:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.KRW;
                case CurrencyIsoCode.KWD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.KWD;
                case CurrencyIsoCode.KYD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.KYD;
                case CurrencyIsoCode.KZT:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.KZT;
                case CurrencyIsoCode.LAK:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.LAK;
                case CurrencyIsoCode.LBP:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.LBP;
                case CurrencyIsoCode.LKR:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.LKR;
                case CurrencyIsoCode.LRD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.LRD;
                case CurrencyIsoCode.LSL:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.LSL;
                case CurrencyIsoCode.LYD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.LYD;
                case CurrencyIsoCode.MAD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.MAD;
                case CurrencyIsoCode.MDL:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.MDL;
                case CurrencyIsoCode.MGA:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.MGA;
                case CurrencyIsoCode.MKD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.MKD;
                case CurrencyIsoCode.MMK:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.MMK;
                case CurrencyIsoCode.MNT:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.MNT;
                case CurrencyIsoCode.MOP:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.MOP;
                case CurrencyIsoCode.MRO:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.MRO;
                case CurrencyIsoCode.MUR:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.MUR;
                case CurrencyIsoCode.MVR:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.MVR;
                case CurrencyIsoCode.MWK:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.MWK;
                case CurrencyIsoCode.MXN:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.MXN;
                case CurrencyIsoCode.MYR:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.MYR;
                case CurrencyIsoCode.MZN:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.MZN;
                case CurrencyIsoCode.NAD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.NAD;
                case CurrencyIsoCode.NGN:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.NGN;
                case CurrencyIsoCode.NIO:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.NIO;
                case CurrencyIsoCode.NOK:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.NOK;
                case CurrencyIsoCode.NPR:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.NPR;
                case CurrencyIsoCode.NZD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.NZD;
                case CurrencyIsoCode.OMR:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.OMR;
                case CurrencyIsoCode.PAB:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.PAB;
                case CurrencyIsoCode.PEN:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.PEN;
                case CurrencyIsoCode.PGK:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.PGK;
                case CurrencyIsoCode.PHP:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.PHP;
                case CurrencyIsoCode.PKR:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.PKR;
                case CurrencyIsoCode.PLN:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.PLN;
                case CurrencyIsoCode.PYG:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.PYG;
                case CurrencyIsoCode.QAR:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.QAR;
                case CurrencyIsoCode.RON:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.RON;
                case CurrencyIsoCode.RSD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.RSD;
                case CurrencyIsoCode.RUB:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.RUB;
                case CurrencyIsoCode.RWF:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.RWF;
                case CurrencyIsoCode.SAR:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.SAR;
                case CurrencyIsoCode.SBD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.SBD;
                case CurrencyIsoCode.SCR:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.SCR;
                case CurrencyIsoCode.SDG:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.SDG;
                case CurrencyIsoCode.SEK:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.SEK;
                case CurrencyIsoCode.SGD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.SGD;
                case CurrencyIsoCode.SHP:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.SHP;
                case CurrencyIsoCode.SLL:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.SLL;
                case CurrencyIsoCode.SOS:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.SOS;
                case CurrencyIsoCode.SRD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.SRD;
                case CurrencyIsoCode.SSP:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.SSP;
                case CurrencyIsoCode.STD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.STD;
                case CurrencyIsoCode.SVC:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.SVC;
                case CurrencyIsoCode.SYP:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.SYP;
                case CurrencyIsoCode.SZL:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.SZL;
                case CurrencyIsoCode.THB:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.THB;
                case CurrencyIsoCode.TJS:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.TJS;
                case CurrencyIsoCode.TMT:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.TMT;
                case CurrencyIsoCode.TND:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.TND;
                case CurrencyIsoCode.TOP:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.TOP;
                case CurrencyIsoCode.TRY:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.TRY;
                case CurrencyIsoCode.TTD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.TTD;
                case CurrencyIsoCode.TWD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.TWD;
                case CurrencyIsoCode.TZS:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.TZS;
                case CurrencyIsoCode.UAH:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.UAH;
                case CurrencyIsoCode.UGX:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.UGX;

                case CurrencyIsoCode.UYU:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.UYU;
                case CurrencyIsoCode.UZS:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.UZS;
                case CurrencyIsoCode.VEF:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.VEF;
                case CurrencyIsoCode.VND:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.VND;
                case CurrencyIsoCode.VUV:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.VUV;
                case CurrencyIsoCode.WST:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.WST;
                case CurrencyIsoCode.XAF:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.XAF;
                case CurrencyIsoCode.XCD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.XCD;
                case CurrencyIsoCode.XOF:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.XOF;
                case CurrencyIsoCode.XPF:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.XPF;
                case CurrencyIsoCode.YER:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.YER;
                case CurrencyIsoCode.ZAR:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.ZAR;
                case CurrencyIsoCode.ZMW:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.ZMW;
                case CurrencyIsoCode.ZWL:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.ZWL;
                default:
                    return null;
            }
        }
            
    }
}