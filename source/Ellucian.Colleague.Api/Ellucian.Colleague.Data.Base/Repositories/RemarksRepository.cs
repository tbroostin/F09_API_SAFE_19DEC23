/*Copyright 2016-2020 Ellucian Company L.P. and its affiliates.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using slf4net;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Web.Dependency;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Dmi.Runtime;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class RemarkRepository : BaseColleagueRepository, IRemarkRepository
    {
        readonly int readSize;
        RepositoryException exception = null;

        public RemarkRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
            this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Update RemarkA
        /// </summary>
        /// <param name="remark"></param>
        /// <returns>Remark domain entity</returns>
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

        /// <summary>
        /// DeleteRemark
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
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
                throw new KeyNotFoundException(message);
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
            string criteria = "WITH REMARKS.DONOR.ID NE '' AND WITH REMARKS.TEXT NE ''";

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

            if (remarksData == null)
            {
                return new Tuple<IEnumerable<Remark>, int>(remarks, 0);
            }

            remarks = await BuildRemarksAsync(remarksData.ToList());

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
                throw new KeyNotFoundException("No comment was found for GUID " + guid);
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("No comment was found for GUID " + guid);
            }

            if (foundEntry.Value.Entity != "REMARKS")
            {
                if (exception == null)
                    exception = new RepositoryException();

                exception.AddError(new RepositoryError("GUID.Wrong.Type", "GUID " + guid + " has different entity, " + foundEntry.Value.Entity + ", than expected, REMARKS"));
                throw exception;
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Get a single remark using an ID
        /// </summary>
        /// <param name="id">The remark ID</param>
        /// <returns>The remark</returns>
        public async Task<Remark> GetRemarkAsync(string id)
        {

            if (string.IsNullOrEmpty(id))
            {
                throw new RepositoryException("ID is required to get a remark.");
            }

            // Now we have an ID, so we can read the record
            var record = await DataReader.ReadRecordAsync<Remarks>(id);
            if (record == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or remark with ID ", id, " invalid."));
            }
            
            if (string.IsNullOrEmpty(record.RemarksDonorId))
            {
                if (exception == null)
                    exception = new RepositoryException();

                exception.AddError(new RepositoryError("Missing.Required.Property", "The comments record is missing the subjectMatter person, organization or institution."));
            }

            if (string.IsNullOrEmpty(record.RemarksText))
            {
                if (exception == null)
                    exception = new RepositoryException();

                exception.AddError(new RepositoryError("Missing.Required.Property", "The comments record is missing the comment text."));
            }

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            var remark = await BuildRemarksAsync(new List<Remarks>() { record });

            if (remark == null || !remark.Any())
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or remark with ID ", id, " invalid."));
            }

            return remark.FirstOrDefault();
        }

        /// <summary>
        /// Build Remarks
        /// </summary>
        /// <param name="sources"></param>
        /// <returns>Collection of Remark domain entities</returns>
        private async Task<IEnumerable<Remark>> BuildRemarksAsync(List<Remarks> sources)
        {
            var remarkCollection = new List<Remark>();

            if ((sources == null) || (!sources.Any()))
            {
                return remarkCollection.AsEnumerable();
            }

            var personIds = sources.Where(x => !(string.IsNullOrEmpty(x.RemarksDonorId))).Select(y => y.RemarksDonorId);
            if (personIds == null || !personIds.Any())
            {
                if (exception == null)
                    exception = new RepositoryException();

                exception.AddError(new RepositoryError("Missing.Required.Property", "Remarks Records are missing donor ID property."));
            }
            else
            {
                var personCorpIndicatorDictCollection = await GetPersonDictionaryCollectionAsync(personIds);

                foreach (var source in sources)
                {
                    try
                    {
                        remarkCollection.Add(BuildRemark(source, personCorpIndicatorDictCollection));
                    }
                    catch (Exception ex)
                    {
                        if (exception == null)
                            exception = new RepositoryException();

                        exception.AddError(new RepositoryError("Global.Internal.Error", ex.Message));
                    }
                }
            }

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return remarkCollection.AsEnumerable();
        }

        /// <summary>
        /// Build Remark
        /// </summary>
        /// <param name="source"></param>
        /// <param name="personCorpIndicatorDictCollection"></param>
        /// <returns>Remark domain entity</returns>
        private Remark BuildRemark(Remarks source, Dictionary<string, Dictionary<string, string>> personCorpIndicatorDictCollection)
        {
            Remark remark = null;

            if (source == null)
            {
                throw new ArgumentNullException("source", "source required to build remark.");
            }

            var personCorpIndicator = string.Empty;
            var personInstIndicator = string.Empty;

            if (!string.IsNullOrEmpty(source.RemarksDonorId)) 
            {
                if (personCorpIndicatorDictCollection == null)
                {
                    if (exception == null)
                        exception = new RepositoryException();

                    exception.AddError(new RepositoryError("Global.Internal.Error", "Person not found for RemarksDonorId Id: " + source.RemarksDonorId));
                }
                else
                {
                    var personCorpIndicatorDict = new Dictionary<string, string>();
                    var found = personCorpIndicatorDictCollection.TryGetValue(source.RemarksDonorId, out personCorpIndicatorDict);
                    if (found)
                    {
                        personCorpIndicatorDict.TryGetValue("PERSON.CORP.INDICATOR", out personCorpIndicator);
                        personCorpIndicatorDict.TryGetValue("WHERE.USED", out personInstIndicator);
                    }
                }

            }

            remark = new Remark(source.RecordGuid)
            {
                RemarksAuthor = source.RemarksAuthor,
                RemarksDate = source.RemarksDate,
                RemarksText = source.RemarksText,
                RemarksType = source.RemarksType,
                RemarksCode = source.RemarksCode,
                RemarksDonorId = source.RemarksDonorId,
                RemarksIntgEnteredBy = source.RemarksIntgEnteredBy,
                RemarksPersonCorpIndicator = personCorpIndicator != null && personCorpIndicator.Equals("Y", StringComparison.OrdinalIgnoreCase) ? true : false,
                RemarksInstIndicator = personInstIndicator != null && personInstIndicator.Contains("INSTITUTIONS") ? true : false
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

        /// <summary>
        /// Returns a Dictionary from Read of PERSON columns.
        /// </summary>
        /// <param name="personIds"></param>
        /// <returns>Dictionary</returns>
        public async Task<Dictionary<string, Dictionary<string, string>>> GetPersonDictionaryCollectionAsync(IEnumerable<string> personIds)
        {
            return await DataReader.BatchReadRecordColumnsAsync("PERSON", personIds.Distinct().ToArray(), new string[] { "PERSON.CORP.INDICATOR", "WHERE.USED" });
        }
    }
}