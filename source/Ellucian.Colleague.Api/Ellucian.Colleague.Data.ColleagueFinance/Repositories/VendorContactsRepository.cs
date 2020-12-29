/*Copyright 2016-2017 Ellucian Company L.P. and its affiliates.*/

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using OrganizationContact = Ellucian.Colleague.Domain.ColleagueFinance.Entities.OrganizationContact;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class VendorContactsRepository : BaseColleagueRepository, IVendorContactsRepository
    {
        private readonly int readSize;
        // Clear from cache every 20 minutes
        protected const int AllVendorsCacheTimeout = 20;
        protected const string AllVendorContactsFilter = "AllVendorContactsFilter";
        RepositoryException repositoryException = new RepositoryException();
        public static char _SM = Convert.ToChar(DynamicArray.SM);

        public VendorContactsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
            this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Get a list of vendor contacts using criteria
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<OrganizationContact>, int>> GetVendorContactsAsync(int offset, int limit, string vendorId)
        {
            try
            {
                int totalCount = 0;
                string[] subList = null;
                string vendorCacheKey = CacheSupport.BuildCacheKey(AllVendorContactsFilter, vendorId);
                var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                       this,
                       ContainsKey,
                       GetOrAddToCacheAsync,
                       AddOrUpdateCacheAsync,
                       transactionInvoker,
                       vendorCacheKey,
                       "ORGANIZATION.CONTACT",
                       offset,
                       limit,
                       AllVendorsCacheTimeout,
                       async () =>
                       {
                           string criteria = !string.IsNullOrWhiteSpace(vendorId) ? 
                           string.Format( "WITH OCN.VENDOR.CONTACT EQ 'Y' AND OCN.VENDOR.ID EQ '{0}'", vendorId) : 
                                          "WITH OCN.VENDOR.ID NE '' AND OCN.VENDOR.CONTACT EQ 'Y'";

                           return new CacheSupport.KeyCacheRequirements()
                           {
                               limitingKeys = null,
                               criteria = criteria
                           };
                       }
                   );

                if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
                {
                    return new Tuple<IEnumerable<OrganizationContact>, int>(new List<OrganizationContact>(), 0);
                }

                subList = keyCache.Sublist.ToArray();
                totalCount = keyCache.TotalCount.Value;

                //ORGANIZATION.CONTACT
                var orgContactsData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.OrganizationContact>("ORGANIZATION.CONTACT", subList);

                if (orgContactsData == null || !orgContactsData.Any())
                {
                    return new Tuple<IEnumerable<OrganizationContact>, int>(new List<OrganizationContact>(), 0);
                }

                //persons for address, phone info
                List<string> ocnPersonId = orgContactsData.Where(p => !string.IsNullOrWhiteSpace(p.OcnPersonId)).Select(i => i.OcnPersonId).Distinct().ToList();
                IEnumerable<Person> persons = await DataReader.BulkReadRecordAsync<Person>(ocnPersonId.ToArray());

                List<string> ocnAddressId = orgContactsData.Where(p => !string.IsNullOrWhiteSpace(p.OcnAddress)).Select(i => i.OcnAddress).Distinct().ToList();
                IEnumerable<Address> addresses = await DataReader.BulkReadRecordAsync<Address>(ocnAddressId.ToArray());

                //Relationship records
                List<string> relIds = orgContactsData.Where(r => !string.IsNullOrWhiteSpace(r.OcnRelationship)).Select(t => t.OcnRelationship).Distinct().ToList();
                IEnumerable<Relationship> reletionships = await DataReader.BulkReadRecordAsync<Relationship>(relIds.ToArray());

                List<OrganizationContact> entities = new List<OrganizationContact>();
                foreach (var orgContact in orgContactsData)
                {
                    OrganizationContact entity = BuildOrganizationContact(orgContact, persons, reletionships, addresses);
                    entities.Add(entity);
                }

                if (repositoryException.Errors.Any())
                {
                    throw repositoryException;
                }

                return entities != null && entities.Any() ? new Tuple<IEnumerable<OrganizationContact>, int>(entities, totalCount) :
                                                           new Tuple<IEnumerable<OrganizationContact>, int>(new List<OrganizationContact>(), 0);
            }
            catch (ArgumentException e)
            {
                throw e;
            }
            catch (RepositoryException e)
            {
                throw e;
            }

        }       

        /// <summary>
        /// Get a list of vendor contacts for list of vendors
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<OrganizationContact>> GetVendorContactsForVendorsAsync(string[] vendorIds)
        {
            try
            {
                List<OrganizationContact> entities = new List<OrganizationContact>();
                var criteria = string.Format("WITH OCN.VENDOR.CONTACT EQ 'Y' AND OCN.VENDOR.ID EQ '?'");
                var OrgContactIds = await DataReader.SelectAsync("ORGANIZATION.CONTACT", criteria, vendorIds);

                //ORGANIZATION.CONTACT
                var orgContactsData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.OrganizationContact>(OrgContactIds);

                if (orgContactsData == null || !orgContactsData.Any())
                {
                    return entities;
                }

                //persons for address, phone info
                List<string> ocnPersonId = orgContactsData.Where(p => !string.IsNullOrWhiteSpace(p.OcnPersonId)).Select(i => i.OcnPersonId).Distinct().ToList();
                IEnumerable<Person> persons = await DataReader.BulkReadRecordAsync<Person>(ocnPersonId.ToArray());

                List<string> ocnAddressId = orgContactsData.Where(p => !string.IsNullOrWhiteSpace(p.OcnAddress)).Select(i => i.OcnAddress).Distinct().ToList();
                IEnumerable<Address> addresses = await DataReader.BulkReadRecordAsync<Address>(ocnAddressId.ToArray());

                //Relationship records
                List<string> relIds = orgContactsData.Where(r => !string.IsNullOrWhiteSpace(r.OcnRelationship)).Select(t => t.OcnRelationship).Distinct().ToList();
                IEnumerable<Relationship> reletionships = await DataReader.BulkReadRecordAsync<Relationship>(relIds.ToArray());

                var newrepoError = new RepositoryException();

                foreach (var orgContact in orgContactsData)
                {
                    try
                    {
                        OrganizationContact entity = BuildOrganizationContact(orgContact, persons, reletionships, addresses);
                        entities.Add(entity);
                    }
                    catch (RepositoryException ex)
                    {
                        if (ex.Errors != null && ex.Errors.Any())
                        {

                            foreach (var error in ex.Errors)
                            {
                                newrepoError.AddError(
                              new RepositoryError("Bad.Data", error.Message)
                              {
                                  SourceId = orgContact.OcnCorpId
                              });
                            }
                        }
                    }
                   
                }

                if (newrepoError!= null && newrepoError.Errors.Any())
                {
                    throw newrepoError;
                }

                return entities;
            }
            catch (ArgumentException e)
            {
                throw e;
            }
            catch (RepositoryException e)
            {
                throw e;
            }

        }
        /// <summary>
        /// Gets organization contact for a guid.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<OrganizationContact> GetGetVendorContactsByGuidAsync( string guid )
        {
            if( string.IsNullOrWhiteSpace( guid ) )
            {
                throw new ArgumentNullException( "guid", "Guid is required." );
            }

            try
            {
                string id = string.Empty;
                try
                {
                    id = await this.GetOrganizatonContactIdFromGuidAsync( guid );
                    if( string.IsNullOrEmpty( id ) )
                    {
                        repositoryException.AddError( new RepositoryError( "Bad.Data", string.Format( "No vendor contact was found for GUID: '{0}'", guid ) ) );
                        throw repositoryException;
                    }
                }
                catch(RepositoryException)
                {
                    throw;
                }
                catch(Exception)
                {
                    repositoryException.AddError( new RepositoryError( "Bad.Data", string.Format( "No vendor contact was found for GUID: '{0}'", guid ) ) );
                    throw repositoryException;
                }
                //ORGANIZATION.CONTACT
                DataContracts.OrganizationContact orgContactData = null;
                try
                {
                    orgContactData = await DataReader.ReadRecordAsync<DataContracts.OrganizationContact>( id );

                    if( orgContactData == null || ( string.IsNullOrEmpty( orgContactData.OcnVendorContact ) || !orgContactData.OcnVendorContact.Equals( "Y" ) ) )
                    {
                        repositoryException.AddError( new RepositoryError( "Bad.Data", string.Format( "No vendor contact was found for GUID: '{0}'", guid ) ) );
                        throw repositoryException;
                    }
                }
                catch( RepositoryException )
                {
                    throw;
                }
                catch( Exception )
                {
                    repositoryException.AddError( new RepositoryError( "Bad.Data", string.Format( "No vendor contact was found for GUID: '{0}'", guid ) ) );
                    throw repositoryException;
                }
                /*
                    Make sure OCN.CORP.ID has Vendors record.                
                */

                try
                {
                    var vendor = await DataReader.ReadRecordAsync<DataContracts.Vendors>( orgContactData.OcnCorpId );
                    if( vendor == null )
                    {
                        repositoryException.AddError( new RepositoryError( "Guid.Not.Found", string.Format( "Vendor record cannot be found for vendor id '{0}'. guid:'{1}'", orgContactData.OcnCorpId, guid ) ) { Id = guid, SourceId = id  } );
                        throw repositoryException;
                    }
                }
                catch(RepositoryException)
                {
                    throw;
                }
                catch
                {
                    repositoryException.AddError( new RepositoryError( "Guid.Not.Found", string.Format( "Vendor record cannot be found for vendor id '{0}'. guid:'{1}'", orgContactData.OcnCorpId, guid ) ) { Id = guid, SourceId = id } );
                    throw repositoryException;
                }

                //persons for address, phone info                
                Person persons = await DataReader.ReadRecordAsync<Person>( orgContactData.OcnPersonId );
                if( persons == null )
                {
                    repositoryException.AddError( new RepositoryError( "Bad.Data", string.Format( "Person record cannot be found for person Id '{0}'", orgContactData.OcnPersonId ) ) { Id = guid, SourceId = id } );
                    throw repositoryException;
                }
                var addresses = new List<Address>();
                if( !string.IsNullOrEmpty( orgContactData.OcnAddress ) )
                {
                    Address address = await DataReader.ReadRecordAsync<Address>( orgContactData.OcnAddress );
                    if( address == null )
                    {
                        repositoryException.AddError( new RepositoryError( "Bad.Data", string.Format( "Address record cannot be found for address Id '{0}'", orgContactData.OcnAddress ) ) { Id = guid, SourceId = id } );
                        throw repositoryException;
                    }
                    else
                    {
                        addresses.Add( address );
                    }

                }

                //Relationship records
                Relationship reletionships = await DataReader.ReadRecordAsync<Relationship>( orgContactData.OcnRelationship );
                if( reletionships == null )
                {
                    repositoryException.AddError( new RepositoryError( "Bad.Data", string.Format( "Relationship record cannot be found for relationship Id '{0}'", orgContactData.OcnRelationship ) ) { Id = guid, SourceId = id } );
                    throw repositoryException;
                }

                var entity = BuildOrganizationContact( orgContactData, new Collection<Person>() { persons }, new Collection<Relationship>() { reletionships }, addresses );

                if( repositoryException.Errors.Any() )
                {
                    throw repositoryException;
                }
                return entity;
            }
            catch( RepositoryException )
            {
                throw;
            }
        }

        /// <summary>
        /// Builds organization contacts entities.
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="persons"></param>
        /// <param name="defContactRelType"></param>
        /// <returns></returns>
        private OrganizationContact BuildOrganizationContact(DataContracts.OrganizationContact source, IEnumerable<Person> persons, IEnumerable<Relationship> reletionships, IEnumerable<Address> addresses)
        {
            try
            {
                //id, guid
                var entity = new OrganizationContact(source.RecordGuid, source.Recordkey)
                {
                    ContactAddress = source.OcnAddress,
                    //vendor.id
                    VendorId = source.OcnCorpId,
                    //startDate
                    StartDate = source.OcnStartDate,
                    //endDate
                    EndDate = source.OcnEndDate
                };

                //contact.relationshipType
                if (reletionships != null && reletionships.Any() && !string.IsNullOrEmpty(source.OcnRelationship))
                {
                    var relType = reletionships.FirstOrDefault(r => !string.IsNullOrEmpty(source.OcnRelationship) && r.Recordkey.Equals(source.OcnRelationship, StringComparison.InvariantCultureIgnoreCase));
                    if (relType == null)
                    {
                        repositoryException.AddError(
                              new RepositoryError("Bad.Data", "Relationship record cannot be found.")
                              {
                                  Id = source.RecordGuid,
                                  SourceId = source.Recordkey
                              });
                    }
                    else
                    {
                        entity.RelationshipType = relType.RsRelationType;
                    }
                }
                else
                {
                    repositoryException.AddError(
                              new RepositoryError("Bad.Data", "Relationship record cannot be found.")
                              {
                                  Id = source.RecordGuid,
                                  SourceId = source.Recordkey
                              });
                }

                if (persons != null && persons.Any())
                {
                    var person = persons.FirstOrDefault(p => !string.IsNullOrEmpty(source.OcnPersonId) && p.Recordkey.Equals(source.OcnPersonId, StringComparison.InvariantCultureIgnoreCase));
                    if (person == null)
                    {
                        repositoryException.AddError(
                          new RepositoryError("Bad.Data", "Person record cannot be found.")
                          {
                              Id = source.RecordGuid,
                              SourceId = source.Recordkey
                          });
                    }
                    else
                    {
                        //personId
                        entity.ContactPersonGuid = person.RecordGuid;
                        entity.ContactPersonId = person.Recordkey;

                        //person.name
                        if (!string.IsNullOrEmpty(person.PreferredName))
                        {
                            entity.ContactPreferedName = person.PreferredName;
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(person.LastName))
                            {
                                var preferedName =
                                    string.Concat(!string.IsNullOrWhiteSpace(person.Prefix) ? string.Concat(person.Prefix.Trim(), " ") : string.Empty,
                                    !string.IsNullOrWhiteSpace(person.FirstName) ? string.Concat(person.FirstName.Trim(), " ") : string.Empty,
                                    !string.IsNullOrWhiteSpace(person.MiddleName) ? string.Concat(person.MiddleName.Trim().Substring(0, 1).ToUpper(), ". ") : string.Empty,
                                    !string.IsNullOrWhiteSpace(person.LastName) ? string.Concat(person.LastName.Trim(), " ") : string.Empty,
                                    !string.IsNullOrWhiteSpace(person.Suffix) ? person.Suffix.Trim() : string.Empty);
                                entity.ContactPreferedName = preferedName.Trim();
                            }
                            else
                            {
                                repositoryException.AddError(new RepositoryError("Missing.Required.Property", "Person lastname is a required field.") { Id = source.RecordGuid, SourceId = source.Recordkey });
                            }
                        }

                        //contact.phones
                        List<ContactPhoneInfo> contactPhoneInfoList = new List<ContactPhoneInfo>();

                        //PERPHONE
                        if (person.PerphoneEntityAssociation != null && person.PerphoneEntityAssociation.Any())
                        {
                            var personalAddrs = person.PerphoneEntityAssociation
                                               .Where(p => !string.IsNullOrEmpty(p.PersonalPhoneNumberAssocMember) && !string.IsNullOrEmpty(p.PersonalPhoneTypeAssocMember))
                                               .Select(p => new ContactPhoneInfo
                                               {
                                                   PhoneType = p.PersonalPhoneTypeAssocMember,
                                                   PhoneNumber = p.PersonalPhoneNumberAssocMember,
                                                   PhoneExtension = p.PersonalPhoneExtensionAssocMember
                                               }).ToList();
                            if (personalAddrs != null && personalAddrs.Any())
                            {
                                contactPhoneInfoList.AddRange(personalAddrs);
                            }
                        }
                        //PSEASON
                        if (!string.IsNullOrEmpty(source.OcnAddress))
                        {
                            if (person.PseasonEntityAssociation != null && person.PseasonEntityAssociation.Any())
                            {
                                var pSeasonAddrs = person.PseasonEntityAssociation.Where(a => !string.IsNullOrEmpty(a.PersonAddressesAssocMember) && a.PersonAddressesAssocMember.Equals(source.OcnAddress, StringComparison.InvariantCultureIgnoreCase)).ToList();
                                if (pSeasonAddrs != null && pSeasonAddrs.Any())
                                {
                                    pSeasonAddrs.ForEach(p =>
                                    {
                                        if (!string.IsNullOrEmpty(p.AddrLocalPhoneAssocMember))
                                        {
                                            string[] localPhones = p.AddrLocalPhoneAssocMember.Split(_SM);
                                            string[] localPhoneTypes = p.AddrLocalPhoneTypeAssocMember.Split(_SM);
                                            string[] localPhoneExts = p.AddrLocalExtAssocMember.Split(_SM);
                                            for (int i = 0; i < localPhones.Length; i++)
                                            {
                                                ContactPhoneInfo ctPhoneInfo = new ContactPhoneInfo();
                                                ctPhoneInfo.PhoneNumber = localPhones[i];//i.AddrLocalPhoneAssocMember;
                                            ctPhoneInfo.PhoneType = localPhoneTypes[i];//i.AddrLocalPhoneTypeAssocMember;
                                            ctPhoneInfo.PhoneExtension = localPhoneExts[i];//i.AddrLocalExtAssocMember;
                                            contactPhoneInfoList.Add(ctPhoneInfo);
                                            }
                                        }
                                    });
                                }
                            }
                            //get if from the address record associated with the contact person

                            if (addresses != null && addresses.Any())
                            {
                                var address = addresses.FirstOrDefault(p => !string.IsNullOrEmpty(source.OcnAddress) && p.Recordkey.Equals(source.OcnAddress, StringComparison.InvariantCultureIgnoreCase));
                                if (address != null)
                                {
                                    if (address.AdrPhonesEntityAssociation != null && address.AdrPhonesEntityAssociation.Any())
                                    {
                                        var addrPhones = address.AdrPhonesEntityAssociation.
                                                      Where(p => !string.IsNullOrEmpty(p.AddressPhonesAssocMember))
                                                      .Select(p => new ContactPhoneInfo
                                                      {
                                                          PhoneType = p.AddressPhoneTypeAssocMember,
                                                          PhoneNumber = p.AddressPhonesAssocMember,
                                                          PhoneExtension = p.AddressPhoneExtensionAssocMember
                                                      }).ToList();
                                        if (addrPhones != null && addrPhones.Any())
                                        {
                                            contactPhoneInfoList.AddRange(addrPhones);
                                        }
                                    }
                                }
                                else
                                {
                                    repositoryException.AddError(
                                     new RepositoryError("Bad.Data", "Address record cannot be found.")
                                     {
                                         Id = source.RecordGuid,
                                         SourceId = source.Recordkey
                                     });
                                }
                            }
                        }
                        if (contactPhoneInfoList != null && contactPhoneInfoList.Any())
                            entity.PhoneInfos = contactPhoneInfoList;
                    }
                }
                else
                {
                    repositoryException.AddError(
                         new RepositoryError("Bad.Data", "Person record cannot be found.")
                         {
                             Id = source.RecordGuid,
                             SourceId = source.Recordkey
                         });
                }
                return entity;
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Get the record key from a GUID.
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetOrganizatonContactIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required.");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("Vendor Contacts GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("Vendor Contacts GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "ORGANIZATION.CONTACT")
            {
                throw new ArgumentException("GUID " + guid + " has different entity, " + foundEntry.Value.Entity + ", than expected, ORGANIZATION.CONTACT");
            }

            return foundEntry.Value.PrimaryKey;
        }

        #region Create vendor contact initiation process
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<Tuple<OrganizationContact, string>> CreateVendorContactInitiationProcessAsync(OrganizationContactInitiationProcess entity)
        {
            if (entity == null)
            {
                throw new RepositoryException("Must provide a organization contact initiation process body.");
            }

            ProcessVendorContactRequest updateRequest = await BuildOrganizationContactInitiationProcessUpdateRequest(entity);
            var extendedDataTuple = GetEthosExtendedDataLists();

            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                updateRequest.ExtendedNames = extendedDataTuple.Item1;
                updateRequest.ExtendedValues = extendedDataTuple.Item2;
            }

            // write the  data
            var updateResponse = await transactionInvoker.ExecuteAsync<ProcessVendorContactRequest, ProcessVendorContactResponse>(updateRequest);

            if (updateResponse.ErrorOccurred)
            {
                var exception = new RepositoryException();
                foreach (var error in updateResponse.ProcessRelationshipRequestErrors)
                {

                    exception.AddError(new RepositoryError("Create.Update.Exception", string.Concat(error.ErrorCodes, " - ", error.ErrorMessages))
                    {
                        //TODO
                        SourceId = updateRequest.OrgContactGuid,
                        Id = updateRequest.OrgContactId
                    });
                }
                throw exception;
            }
            // get the updated entity from the database
            OrganizationContact orgContact = null;
            string personMatchReqGuid = null;
            if (!string.IsNullOrEmpty(updateResponse.OrgContactId))
            {
                orgContact = await GetGetVendorContactsByGuidAsync(updateResponse.OrgContactGuid);
            }
            else
            {
                personMatchReqGuid = updateResponse.PersonMatchRequestGuid;
            }
            return new Tuple<OrganizationContact, string>(orgContact, personMatchReqGuid);
        }

        /// <summary>
        /// Builds ctx request.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task<ProcessVendorContactRequest> BuildOrganizationContactInitiationProcessUpdateRequest(OrganizationContactInitiationProcess entity)
        {
            var exception = new RepositoryException();
            var defaults = await DataReader.ReadRecordAsync<Dflts>("CORE.PARMS", "DEFAULTS");
            if ( defaults == null || (defaults != null && string.IsNullOrEmpty(defaults.DfltsOrgContactRelType)))
            {
                exception.AddError(new RepositoryError("Validation.Exception", "The default Organization Contact Relation Type is required on RELP in order to create a vendor contact."));
                throw exception;
            }

            var request = new ProcessVendorContactRequest()
            {
                VendorId = entity.VendorId,
                ContactPersonId = entity.PersonId,
                FirstName = entity.FirstName,
                MiddleName = entity.MiddleName,
                LastName = entity.LastName,
                RequestType = entity.RequestType
            };
            //Phone Info
            if (entity.PhoneInfos != null && entity.PhoneInfos.Any())
            {
                foreach (var phInfo in entity.PhoneInfos)
                {
                    if (phInfo != null && !string.IsNullOrWhiteSpace(phInfo.PhoneNumber))
                    {
                        request.ProcessVendorContactPhones.Add(new ProcessVendorContactPhones()
                        {
                            PhoneNumbers = phInfo.PhoneNumber,
                            PhoneTypes = phInfo.PhoneType,
                            PmrPhoneExtensions = phInfo.PhoneExtension
                        });
                    }
                }
            }

            //Email Info
            if (entity.EmailInfo != null && !string.IsNullOrWhiteSpace(entity.EmailInfo.EmailAddress))
            {
                request.ProcessVendorContactEmails.Add(new ProcessVendorContactEmails()
                {
                    EmailAddresses = entity.EmailInfo.EmailAddress,
                    EmailTypes = entity.EmailInfo.EmailType
                });
            }
            
            return request;
        }

        #endregion
    }
}