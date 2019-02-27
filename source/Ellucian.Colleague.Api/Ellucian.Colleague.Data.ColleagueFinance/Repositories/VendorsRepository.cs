/*Copyright 2016-2017 Ellucian Company L.P. and its affiliates.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using slf4net;
using Ellucian.Web.Dependency;
using Ellucian.Colleague.Domain.Exceptions;
using Vendors = Ellucian.Colleague.Domain.ColleagueFinance.Entities.Vendors;
using System.Text;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class VendorsRepository : BaseColleagueRepository, IVendorsRepository
    {
        private readonly int readSize;

        public VendorsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {

            CacheTimeout = Level1CacheTimeoutValue;

            this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;

        }

        /// <summary>
        /// Get the GUID for a vendor using its ID
        /// </summary>
        /// <param name="id">Section ID</param>
        /// <returns>Section GUID</returns>
        public async Task<string> GetVendorGuidFromIdAsync(string id)
        {
            try
            {
                return await GetGuidFromRecordInfoAsync("VENDORS", id);
            }
            catch (RepositoryException ex)
            {
                ex.AddError(new RepositoryError("Vendors.Guid.NotFound", "GUID not found for vendor id: " + id));
                throw ex;
            }
        }

        /// <summary>
        /// Get a list of vendors using criteria
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Vendors>, int>> GetVendorsAsync(int offset, int limit, string vendorDetail = "", List<string> classifications = null,
            List<string> statuses = null, List<string> relatedReference = null, List<string> types = null)
        {
            string criteria = "";

            var criteriaBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(vendorDetail))
            {
                criteriaBuilder.AppendFormat("WITH VENDORS.ID = '{0}'", vendorDetail);
            }
            if (classifications != null && classifications.Any())
            {
                if (criteriaBuilder.Length > 0)
                {
                    criteriaBuilder.Append(" AND ");
                }
                int i = 0;
                foreach (var classification in classifications)
                {
                    if ( i == 0)
                        criteriaBuilder.Append("WITH ");
                    else
                        criteriaBuilder.Append(" AND ");
                    criteriaBuilder.AppendFormat("VEN.TYPES = '{0}'", classification);
                    i++;
                }
            }
            if ((statuses != null) && (statuses.Any()))
            {
                foreach (var status in statuses)
                {
                    switch (status.ToLower())
                    {
                        case "active":
                            if (criteriaBuilder.Length > 0) criteriaBuilder.Append(" AND ");
                            criteriaBuilder.Append("WITH VEN.ACTIVE.FLAG = 'Y'");
                            break;
                        case "holdpayment":
                            if (criteriaBuilder.Length > 0) criteriaBuilder.Append(" AND ");
                            criteriaBuilder.Append("WITH VEN.STOP.PAYMENT.FLAG = 'Y'");
                            break;
                        case "approved":
                            if (criteriaBuilder.Length > 0) criteriaBuilder.Append(" AND ");
                            criteriaBuilder.Append("WITH VEN.APPROVAL.FLAG = 'Y'");
                           break;
                    }
                }
            }

            string typesCriteria = string.Empty;
            if (types != null && types.Any())
            {
                foreach (var type in types.Distinct())
                {
                    if (!string.IsNullOrEmpty(type))
                    {
                        switch (type.ToLower())
                        {
                            case "eprocurement":
                                if (criteriaBuilder.Length > 0)
                                {
                                    typesCriteria = " AND WITH VEN.CATEGORIES = 'EP'";
                                }
                                else
                                {
                                    typesCriteria = "WITH VEN.CATEGORIES = 'EP'";
                                }
                                criteriaBuilder.Append(typesCriteria);
                                continue;
                            case "travel":
                                if (criteriaBuilder.Length > 0)
                                {
                                    typesCriteria = " AND WITH VEN.CATEGORIES = 'TR'";
                                }
                                else
                                {
                                    typesCriteria = "WITH VEN.CATEGORIES = 'TR'";
                                }
                                criteriaBuilder.Append(typesCriteria);
                                continue;
                        }
                    }
                }
            }

            if (criteriaBuilder.Length > 0)
            {
                criteria = criteriaBuilder.ToString();
            }

            string[] vendorsIds = null;

            if ((relatedReference != null) && (relatedReference.Any()))
            {
                //at the moment only perentVendor is available. So we'll look for only that type, even though
                // this is an array.

                string personCriteria = "WITH PERSON.CORP.INDICATOR = 'Y' AND WITH PARENTS NE ''";

                var corpIds = await DataReader.SelectAsync("PERSON", personCriteria);
                if (corpIds.Any())
                    vendorsIds = await DataReader.SelectAsync("VENDORS", corpIds, criteria);

            }
            else
            {
                vendorsIds = await DataReader.SelectAsync("VENDORS", criteria);
            }

            if ((vendorsIds == null) || (!vendorsIds.Any()))
            {
                return new Tuple<IEnumerable<Vendors>, int>(new List<Vendors>(), 0);
            }

            var totalCount = vendorsIds.Count();
            Array.Sort(vendorsIds);
            var subList = vendorsIds.Skip(offset).Take(limit).ToArray();

            var vendorsData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>("VENDORS", subList);
            {
                if (vendorsData == null)
                {
                    throw new KeyNotFoundException("No records selected from Vendors file in Colleague.");
                }
            }
            var personsData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", subList);
            {
                if (personsData == null)
                {
                    throw new KeyNotFoundException("No records selected from Persons file in Colleague.");
                }
            }
            var corpContract = await DataReader.BulkReadRecordAsync<Corp>("PERSON", subList);

            var vendors = BuildVendors(vendorsData.ToList(), personsData.ToList(), corpContract.ToList());

            return new Tuple<IEnumerable<Vendors>, int>(vendors, totalCount);
        }

        /// <summary>
        /// Get a single vendor using a GUID
        /// </summary>
        /// <param name="guid">The Vendor guid</param>
        /// <returns>The vendor domain entity</returns>
        public async Task<Vendors> GetVendorsByGuidAsync(string guid)
        {
            try
            {
                string id = await GetVendorIdFromGuidAsync(guid);
                if (string.IsNullOrEmpty(id))
                {
                    throw new KeyNotFoundException(string.Concat("Id not found for vendor guid:", guid));
                }
                return await GetVendorsAsync(id);
            }
            catch (Exception e)
            {
                throw new KeyNotFoundException("Vendors GUID " + guid + " lookup failed.");
            }
        }

        /// <summary>
        /// Update an existing Vendors domain entity
        /// </summary>
        /// <param name="vendorsEntity">Vendors domain entity</param>
        /// <returns>Vendors domain entity</returns>
        public async Task<Vendors> UpdateVendorsAsync(Vendors vendorsEntity)
        {
            if (vendorsEntity == null)
                throw new ArgumentNullException("vendorsEntity", "Must provide a vendorsEntity to update.");
            if (string.IsNullOrWhiteSpace(vendorsEntity.Guid))
                throw new ArgumentNullException("vendorsEntity", "Must provide the guid of the vendorsEntity to update.");

            // verify the GUID exists to perform an update.  If not, perform a create instead
            var vendorsId = await this.GetVendorIdFromGuidAsync(vendorsEntity.Guid);
            if (!string.IsNullOrEmpty(vendorsId))
            {
                
                var updateRequest = await BuildVendorsUpdateRequestAsync(vendorsEntity);

                var extendedDataTuple = GetEthosExtendedDataLists();
                if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
                {
                    updateRequest.ExtendedNames = extendedDataTuple.Item1;
                    updateRequest.ExtendedValues = extendedDataTuple.Item2;
                }

                // write the  data
                var updateResponse = await transactionInvoker.ExecuteAsync<CreateUpdateVendorRequest, CreateUpdateVendorResponse>(updateRequest);

                if (updateResponse.Errors.Any())
                {
                    var errorMessage = string.Format("Error(s) occurred updating vendors '{0}':", vendorsEntity.Guid);
                    var exception = new RepositoryException(errorMessage);
                    updateResponse.Errors.ForEach(e => exception.AddError(new RepositoryError(e.ErrorCodes, e.ErrorMessages)));
                    logger.Error(errorMessage);
                    throw exception;
                }

                // get the updated entity from the database
                return await GetVendorsByGuidAsync(vendorsEntity.Guid);
            }

            // perform a create instead
            return await CreateVendorsAsync(vendorsEntity);
       
        }

        /// <summary>
        /// Create a new Vendors domain entity
        /// </summary>
        /// <param name="vendorsEntity">Vendors domain entity</param>
        /// <returns>Vendors domain entity</returns>
        public async Task<Vendors> CreateVendorsAsync(Vendors vendorsEntity)
        {
            if (vendorsEntity == null)
                throw new ArgumentNullException("vendorsEntity", "Must provide a vendorsEntity to create.");

            var createRequest = await BuildVendorsUpdateRequestAsync(vendorsEntity);
            createRequest.VendorId = string.Empty;

            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                createRequest.ExtendedNames = extendedDataTuple.Item1;
                createRequest.ExtendedValues = extendedDataTuple.Item2;
            }

            var createResponse = await transactionInvoker.ExecuteAsync<CreateUpdateVendorRequest, CreateUpdateVendorResponse>(createRequest);

            if (createResponse.Errors.Any())
            {
                var errorMessage = string.Format("Error(s) occurred updating vendors '{0}':", vendorsEntity.Guid);
                var exception = new RepositoryException(errorMessage);
                createResponse.Errors.ForEach(e => exception.AddError(new RepositoryError(e.ErrorCodes, e.ErrorMessages)));
                logger.Error(errorMessage);
                throw exception;
            }

            // get the newly created record from the database
            return await GetVendorsByGuidAsync(createResponse.VendorGuid); 
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetVendorIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] {new GuidLookup(guid)});
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("Vendors GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("Vendors GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "VENDORS")
            {
                throw new ArgumentException("GUID " + guid + " has different entity, " + foundEntry.Value.Entity + ", than expected, VENDORS");
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Get a single vendor using an ID
        /// </summary>
        /// <param name="id">The vendor GUID</param>
        /// <returns>The vendor</returns>
        public async Task<Vendors> GetVendorsAsync(string id)
        {
            Vendors vendor = null;

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a vendor.");
            }

            // Now we have an ID, so we can read the record
            var record = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors>(id);
            if (record == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found for vendor with ID ", id, "invalid."));
            }

            var personRecord = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(id);
            if (personRecord == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found for person with ID ", id, "invalid."));
            }

            var corpRecord = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", id);
            
            // Build the vendor data
            vendor = BuildVendors(record, personRecord, corpRecord);

            return vendor;
        }

        /// <summary>
        /// Build a collection of vendor domain entities from data contracts
        /// </summary>
        /// <param name="sources">Vendors data contracts</param>
        /// <param name="persons">Persons data contracst</param>
        /// <param name="corp"></param>
        /// <returns>collection of vendor domain entities</returns>
        private IEnumerable<Vendors> BuildVendors(List<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors> sources,
            List<Ellucian.Colleague.Data.Base.DataContracts.Person> persons,
            List<Ellucian.Colleague.Data.Base.DataContracts.Corp> corp)
        {
            var vendorCollection = new List<Vendors>();
            var cor = new Corp();
            foreach (var source in sources)
            {
                var person = persons.FirstOrDefault(p => p.Recordkey == source.Recordkey);
                if ( corp != null)
                    cor = corp.FirstOrDefault(p => p.Recordkey == source.Recordkey);
                vendorCollection.Add(BuildVendors(source, person, cor));
            }

            return vendorCollection.AsEnumerable();
        }

        /// <summary>
        /// Build a vendor domain entity from data contracts
        /// </summary>
        /// <param name="source">Vendors data contract</param>
        /// <param name="person">Persons data contract</param>
        /// <param name="corp">Corp data contract</param>
        /// <returns>Vendors domain entity</returns>
        private Vendors BuildVendors(Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Vendors source,
            Ellucian.Colleague.Data.Base.DataContracts.Person person,
            Ellucian.Colleague.Data.Base.DataContracts.Corp corp)
        {
            Vendors vendor = null;

            if (source == null)
            {
                throw new ArgumentNullException("source", "source required to build vendor.");
            }

            vendor = new Vendors(source.RecordGuid);
            vendor.Id = source.Recordkey;
            vendor.ActiveFlag = source.VenActiveFlag;
            if (source.VendorsAddDate.HasValue)
            {
                vendor.AddDate = source.VendorsAddDate;
            }
            vendor.ApTypes = source.VenApTypes;
            vendor.ApprovalFlag = source.VenApprovalFlag;
            vendor.CurrencyCode = source.VenCurrencyCode;
            vendor.Misc = source.VenMisc;
            vendor.StopPaymentFlag = source.VenStopPaymentFlag;
            vendor.Terms = source.VenTerms;
            vendor.Types = source.VenTypes;
            vendor.Categories = source.VenCategories;
            if (!string.IsNullOrWhiteSpace(source.VenComments))
            {
                vendor.Comments = source.VenComments;
            }


            if ((corp != null) && (corp.CorpParents != null && corp.CorpParents.Any()))
            {
                vendor.CorpParent = corp.CorpParents;
            }

            if (person != null)
            {
                vendor.IsOrganization = (person.PersonCorpIndicator == "Y");
            }

            if (source.VenIntgHoldReasons != null && source.VenIntgHoldReasons.Any())
            {
                vendor.IntgHoldReasons = source.VenIntgHoldReasons;
            }
            return vendor;
        }

        /// <summary>
        /// Create an CreateUpdateVendorRequest from a Vendor domain entity
        /// </summary>
        /// <param name="vendorEntity">Vendor domain entity</param>
        /// <returns>CreateUpdateVendorRequest transaction object</returns>
        private async Task<CreateUpdateVendorRequest> BuildVendorsUpdateRequestAsync(Vendors vendorEntity)
        {
            var request = new CreateUpdateVendorRequest();

            if (vendorEntity.IsOrganization)
            {
                request.OrgId = vendorEntity.Id;
            }
            else
            {
                var personRecord = await DataReader.ReadRecordAsync<Base.DataContracts.Person>(vendorEntity.Id);
                if (personRecord == null)
                {
                    throw new KeyNotFoundException(string.Concat("Record not found for person with ID ", vendorEntity.Id, "invalid."));
                }
                if (personRecord.PersonCorpIndicator == "Y")
                {
                    request.InstitutionId = vendorEntity.Id;
                }
                else
                {
                    request.PersonId = vendorEntity.Id;
                }
            }

            request.ClassificationsId = vendorEntity.Types;
            request.Comments = vendorEntity.Comments;
            request.DefaultCurrency = vendorEntity.CurrencyCode;
            
            request.PaymentSourcesId = vendorEntity.ApTypes;
            request.PaymentTermsId = vendorEntity.Terms;

            var vendorStatuses = new List<string>();
            if (vendorEntity.ActiveFlag == "Y")
                vendorStatuses.Add("active");
            if (vendorEntity.StopPaymentFlag == "Y")
                vendorStatuses.Add("holdPayment");
            if (vendorEntity.ApprovalFlag == "Y")
                vendorStatuses.Add("approved");

            request.Statuses = vendorStatuses;
            request.VendorGuid = vendorEntity.Guid;

            if ( vendorEntity.IntgHoldReasons != null && vendorEntity.IntgHoldReasons.Any())
                request.VendorHoldReasonsId = vendorEntity.IntgHoldReasons;

            request.VendorTypes = vendorEntity.Categories;   

            request.VendorId = vendorEntity.Id;
            return request;
        }

    }
}