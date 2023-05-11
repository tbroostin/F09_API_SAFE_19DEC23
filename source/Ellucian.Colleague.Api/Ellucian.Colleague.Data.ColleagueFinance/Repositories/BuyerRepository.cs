// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Web.Cache;
using slf4net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Web.Http.Configuration;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Data.Base.Repositories;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    public class BuyerRepository: PersonRepository, IBuyerRepository
    {
        private RepositoryException repositoryException = new RepositoryException();
        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerRepository"/> class.
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="transactionFactory">The transaction factory.</param>
        /// <param name="logger">The logger.</param>
        public BuyerRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger, apiSettings)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Get all staff records and convert it to the Buyer Entity
        /// </summary>
        /// <typeparam name="Buyer"></typeparam>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Buyer>, int>> GetBuyersAsync(int offset, int limit, bool bypassCache = false)
        {
            List<Buyer> buyers = new List<Buyer>();

            var staffIds = await DataReader.SelectAsync("STAFF", string.Empty);
            if ((staffIds == null || !staffIds.Any()))
            {
                return new Tuple<IEnumerable<Buyer>, int>(null, 0);
            }

            int totalRecords = staffIds.Count();

            Array.Sort(staffIds);

            var subList = staffIds.Skip(offset).Take(limit);

            if (subList != null && subList.Any())
            {
                try
                {
                    var personRecords = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(subList.ToArray(), bypassCache);

                    var staffStatusesValidationTable = await GetStaffStatusesValcode();

                    var staffData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Staff>(subList.ToArray(), bypassCache);
                    foreach (var staff in staffData)
                    {
                        Buyer buyerEntity = await GetBuyerEntity(staff, personRecords.ToList(), staffStatusesValidationTable);
                        buyers.Add(buyerEntity);
                    }
                }
                catch (Exception ex)
                {
                    throw new ColleagueWebApiDtoException("Error occurred while getting staff records.", ex);
                }
            }
            if (repositoryException != null && repositoryException.Errors != null && repositoryException.Errors.Any())
            {
                throw repositoryException;
            }
            return new Tuple<IEnumerable<Buyer>, int>(buyers, totalRecords);
        }

        public async Task<Buyer> GetBuyerAsync(string guid)
        {
            var entity = await this.GetRecordInfoFromGuidAsync(guid);
            if (entity == null || entity.Entity != "STAFF")
            {
                throw new KeyNotFoundException(string.Format("Buyer not found for GUID {0}", guid));
            }

            var staffId = entity.PrimaryKey;
            if (string.IsNullOrWhiteSpace(staffId))
            {
                throw new KeyNotFoundException("Guid " + guid + "does not exist");
            }
            var staffRecord = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Staff>("STAFF", staffId);
            if (staffRecord == null)
            {
                throw new KeyNotFoundException("Buyer not found with GUID " + guid);
            }
            var personRecord = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", staffId);
            List<Ellucian.Colleague.Data.Base.DataContracts.Person> personRecords = new List<Ellucian.Colleague.Data.Base.DataContracts.Person>();
            personRecords.Add(personRecord);
            var staffStatusesValidationTable = await GetStaffStatusesValcode();
            var buyerEntity =  await GetBuyerEntity(staffRecord, personRecords, staffStatusesValidationTable);
            if (repositoryException != null && repositoryException.Errors != null && repositoryException.Errors.Any())
            {
                throw repositoryException;
            }
            return buyerEntity;

        }

        private async Task<Buyer> GetBuyerEntity(Ellucian.Colleague.Data.Base.DataContracts.Staff staff, List<Ellucian.Colleague.Data.Base.DataContracts.Person> StaffEntities, ApplValcodes staffStatusesValidationTable)
        {
            if (string.IsNullOrEmpty(staff.RecordGuid))
            {
                repositoryException.AddError(new RepositoryError("GUID.Not.Found", string.Concat("No Guid found, Entity:'STAFF', Record ID:'", staff.Recordkey, "'")
                    ));        
            }
            Buyer BuyerEntity = new Buyer()
            {
                RecordKey = staff.Recordkey,
                Guid = staff.RecordGuid,
                StartOn = staff.StaffAddDate,
            };
            
            // update IsActive flag on staff record if status indicates Active
            ApplValcodesVals staffStatus = null;
            if (staffStatusesValidationTable != null && staffStatusesValidationTable.ValsEntityAssociation != null && !string.IsNullOrEmpty(staff.StaffStatus))
            {
                staffStatus = staffStatusesValidationTable.ValsEntityAssociation.FirstOrDefault(v => v.ValInternalCodeAssocMember == staff.StaffStatus);
            }

            // Status must have a special processing code of "A" to be considered "Active"
            if (staffStatus != null && staffStatus.ValActionCode1AssocMember == "A")
            {                
                BuyerEntity.Status = "active";
            }
            else
            {
                BuyerEntity.Status = "inactive";
            }
            try
            {
                BuyerEntity.PersonGuid = await GetGuidFromRecordInfoAsync("PERSON", staff.Recordkey);
            }
            catch (Exception ex)
            {
                repositoryException.AddError(new RepositoryError("GUID.Not.Found", string.Concat("No Guid found, Entity:'PERSON', Record ID:'", staff.Recordkey, "'")
                   ));
            }
            Ellucian.Colleague.Data.Base.DataContracts.Person StaffEntity = StaffEntities.FirstOrDefault(x => x.Recordkey == BuyerEntity.RecordKey);

            if (StaffEntity != null)
            {
                BuyerEntity.Name = string.Concat(StaffEntity.FirstName, " ", StaffEntity.LastName);
            }

            return BuyerEntity;
        }

        /// <summary>
        /// Gets Staff Statuses Valcodes
        /// </summary>
        /// <returns>STAFF.STATUSES Valcodes</returns>
        private async Task<ApplValcodes> GetStaffStatusesValcode()
        {
            // Get the STAFF.STATUSES valcode 
            var staffStatusesValidationTable = new ApplValcodes();
            try
            {
                staffStatusesValidationTable = await GetOrAddToCacheAsync<ApplValcodes>("StaffStatuses",
                    async () =>
                    {
                        ApplValcodes staffStatusesValTable = await DataReader.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "STAFF.STATUSES");
                        return staffStatusesValTable;
                    }, Level1CacheTimeoutValue);
            }
            catch (Exception)
            {
                // log the issue and move on. Not likely to happen...
                var errorMessage = "Unable to retrieve STAFF.STATUSES validation table from Colleague.";
                logger.Info(errorMessage);
            }
            return staffStatusesValidationTable;
        }

        /// <summary>
        /// Get the GUID for a buyer using its ID
        /// </summary>
        /// <param name="id">Buyer ID</param>
        /// <returns>Buyer GUID</returns>
        public async Task<string> GetBuyerGuidFromIdAsync(string id)
        {
            try
            {
                return await GetGuidFromRecordInfoAsync("STAFF", id);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (RepositoryException ex)
            {
                ex.AddError(new RepositoryError("Guid NotFound", "GUID not found for buyer " + id));
                throw ex;
            }
        }


       
        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetBuyerIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("Buyers GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("Buyers GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "STAFF")
            {
                throw new KeyNotFoundException("GUID " + guid + " has different entity, " + foundEntry.Value.Entity + ", than expected, STAFF");
            }

            return foundEntry.Value.PrimaryKey;
        }
    }
}
