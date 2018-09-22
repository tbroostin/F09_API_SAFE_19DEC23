/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using slf4net;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Web.Dependency;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class RemarkRepository : BaseColleagueRepository, IRemarkRepository
    {
        readonly int readSize;
        
        public RemarkRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            
            CacheTimeout = Level1CacheTimeoutValue;
            
            this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;

        }
       
        public async Task<Remark> UpdateRemarkAsync(Remark remark)
        {
            if (remark == null)
            {
                throw new ArgumentNullException("remark");
            }

            var extendedDataTuple = GetEthosExtendedDataLists();
            var request = new UpdateRemarkRequest()
            {
                RmkType = remark.RemarksType,
                RmkAuthor = remark.RemarksAuthor,
                RmkDate = remark.RemarksDate,
                RmkCode = remark.RemarksCode,
                RmkText = new List<string>() { remark.RemarksText },
                RmkPersonId = remark.RemarksDonorId,
                RmkGuid = remark.Guid,
                RmkEnteredBy = remark.RemarksIntgEnteredBy
            };

            switch (remark.RemarksPrivateType)
            {
                case ConfidentialityType.Public:
                    request.RmkPrivateFlag = "N";
                    break;
                case ConfidentialityType.Private:
                    request.RmkPrivateFlag = "Y";
                    break;
                default:
                    request.RmkPrivateFlag = "N";
                    break;
            }

            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }
            var response = await transactionInvoker.ExecuteAsync<UpdateRemarkRequest, UpdateRemarkResponse>(request);

            if (response.ErrorMessage != null && response.ErrorMessage.Any())
            {
                var errorMessagesString = string.Join(Environment.NewLine, response.ErrorMessage);
                logger.Error(errorMessagesString);
                throw new ApplicationException(errorMessagesString);
            }

            var createdRemark = await GetRemarkAsync(response.RmkId);

            return createdRemark;
        }

        public async Task DeleteRemarkAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("updatedRemark");
            }
            var existingRemark = await this.GetRemarkByGuidAsync(guid);

            if (existingRemark == null)
            {
                var message = string.Concat("Remark does not exist for GUID:", guid);
                logger.Error(message);
                throw new ApplicationException(message);
            }

            var request = new DeleteRemarksRequest()
            {
                RmkGuid = guid
            };

            var response = await transactionInvoker.ExecuteAsync<DeleteRemarksRequest, DeleteRemarksResponse>(request);

            if (response.ErrorMessages != null && response.ErrorMessages.Any())
            {
                var errorMessagesString = string.Join(Environment.NewLine, response.ErrorMessages);
                logger.Error(errorMessagesString);
                throw new ApplicationException(errorMessagesString);
            }
        }

        /// <summary>
        /// Get a list of remarks using criteria
        /// subjectMatter - find all of the records in REMARKS where the REMARKS.DONOR.ID 
        ///     matches the person or organization ID corresponding to the guid found in subjectMatter.person.id or subjectMatter.organization.id.
        /// commentSubjectArea - find all of the records in REMARKS where the REMARKS.TYPE 
        ///     matches the code corresponding to the guid in commentSubjectArea.id.
        /// </summary>
        /// <param name="subjectMatterId">subjectMatter</param>
        /// <param name="commentSubjectAreaId">commentSubjectArea</param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Remark>, int>> GetRemarksAsync(int offset, int limit, string subjectMatterId = "", string commentSubjectAreaId = "")
        {
            IEnumerable<Remark> remarks = new List<Remark>();
            string criteria = "WITH REMARKS.TEXT NE ''";

            if (!string.IsNullOrEmpty(subjectMatterId))
            {

                criteria += " AND WITH REMARKS.DONOR.ID EQ '" + subjectMatterId + "'";
            }
            if (!string.IsNullOrEmpty(commentSubjectAreaId))
            {
               criteria += " AND WITH REMARKS.TYPE EQ '" + commentSubjectAreaId + "'";
            }

            var remarksIds = await DataReader.SelectAsync("REMARKS", criteria);
            var totalCount = remarksIds.Count();
            Array.Sort(remarksIds);
            var subList = remarksIds.Skip(offset).Take(limit).ToArray();

            var remarksData = await DataReader.BulkReadRecordAsync<Remarks>("REMARKS", subList);
            {
                if (remarksData == null)
                {
                    throw new KeyNotFoundException("No records selected from Address in Colleague.");
                }
            };
            remarks = BuildRemarks(remarksData.ToList());

            return new Tuple<IEnumerable<Remark>, int>(remarks, totalCount);
        }


        /// <summary>
        /// Get a single remark using a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>The remark</returns>

        public async Task<Remark> GetRemarkByGuidAsync(string guid)
        {
            return await GetRemarkAsync(await GetRemarkFromGuidAsync(guid));
        }


        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetRemarkFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("Remark GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("Remark GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "REMARKS")
            {
                throw new RepositoryException("GUID " + guid + " has different entity, " + foundEntry.Value.Entity + ", than expected, REMARKS");
            }

            return foundEntry.Value.PrimaryKey;
        }



        /// <summary>
        /// Get a single remark using an ID
        /// </summary>
        /// <param name="id">The remark GUID</param>
        /// <returns>The remark</returns>
        public async Task<Remark> GetRemarkAsync(string id)
        {
            Remark remark = null;

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a remark.");
            }

            // Now we have an ID, so we can read the record
            var record = await DataReader.ReadRecordAsync<Remarks>(id);
            if (record == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or remark with ID ", id, "invalid."));
            }

            // Build the section data
            remark = BuildRemark(record);

            return remark;
        }


        public IEnumerable<Remark> BuildRemarks(List<Remarks> sources)
        {
            var remarkCollection = new List<Remark>();
            foreach (var source in sources)
            {
                remarkCollection.Add(BuildRemark(source));
            }

            return remarkCollection.AsEnumerable();
        }



        public Remark BuildRemark(Remarks source)
        {
            Remark remark = null;

            if (source == null)
            {
                throw new ArgumentNullException("source", "source required to build remark.");
            }

            remark = new Remark(source.RecordGuid)
            {
                RemarksAuthor = source.RemarksAuthor,
                RemarksDate = source.RemarksDate,
                RemarksText = source.RemarksText,
                RemarksType = source.RemarksType,
                RemarksCode = source.RemarksCode,
                RemarksDonorId = source.RemarksDonorId,
                RemarksIntgEnteredBy = source.RemarksIntgEnteredBy
            };

            switch (source.RemarksPrivateFlag)
            {
                case ("N"):
                    remark.RemarksPrivateType = ConfidentialityType.Public;
                    break;
                case ("Y"):
                    remark.RemarksPrivateType = ConfidentialityType.Private;
                    break;
                default:
                    remark.RemarksPrivateType = ConfidentialityType.Public;
                    break;
            }
            return remark;
        }
    }
}