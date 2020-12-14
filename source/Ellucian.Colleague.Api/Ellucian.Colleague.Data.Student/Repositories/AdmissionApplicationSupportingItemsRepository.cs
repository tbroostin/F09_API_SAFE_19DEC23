// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
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

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType]
    public class AdmissionApplicationSupportingItemsRepository : BaseColleagueRepository, IAdmissionApplicationSupportingItemsRepository, IEthosExtended
    {
        public static char _VM = Convert.ToChar(DynamicArray.VM);
        private ApplValcodes _corrStatuses;
        protected const string suppItemsKeysCacheKey = "AllApplicationSupportingItemsKeys";
        protected const int suppItemsCacheTimeout = 20; // Clear from cache every 20 minutes
        private RepositoryException repositoryErrors = new RepositoryException();

        public AdmissionApplicationSupportingItemsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
       }
      
        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<KeyValuePair<string, GuidLookupResult>> GetAdmissionApplicationSupportingItemsIdFromGuidAsync(string guid)
        {                        
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("AdmissionApplicationSupportingItems GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("AdmissionApplicationSupportingItems GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "MAILING")
            {
                throw new RepositoryException("GUID " + guid + " has different entity, " + foundEntry.Value.Entity + ", than expected, ACAD.CREDENTIALS");
            }

            return foundEntry;
        }

        /// <summary>
        /// Using a collection of Student Academic Program ids, get a dictionary collection of associated guids
        /// </summary>
        /// <param name="ids">collection of Student Academic Program ids</param>
        /// <param name="applicationsData">Collection of applications data associated to IDs</param>
        /// <returns>Dictionary consisting of a Student Academic Program (key) and guid (value)</returns>
        private async Task<Dictionary<string, string>> GetAdmissionApplicationSupportingItemsGuidsCollectionAsync(IEnumerable<string> ids, Collection<Applications> applicationsData)
        {
            if ((ids == null) || (ids != null && !ids.Any()))
            {
                return new Dictionary<string, string>();
            }
            var newIds = new List<string>();
            var supportingItemsGuidCollection = new Dictionary<string, string>();
            foreach (var supportingItemId in ids)
            {
                var corresReceivedId = supportingItemId.Split('*');
                var applicationId = corresReceivedId[0] != null ? corresReceivedId[0] : string.Empty;
                
                var application = applicationsData.Where(a => a.Recordkey == applicationId).FirstOrDefault();
                if (application != null && !string.IsNullOrEmpty(application.ApplApplicant))
                {
                    var newId = string.Concat(application.ApplApplicant, "+", supportingItemId);
                    newIds.Add(newId);
                }
            }

            var guidLookup = newIds
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct().ToList()
                .ConvertAll(p => new RecordKeyLookup("MAILING", p.Split('+')[0], "MAILING.ADM.APP.SI.IDX", p.Split('+')[1], false)).ToArray();
            var recordKeyLookupResults = await DataReader.SelectAsync(guidLookup);
            foreach (var recordKeyLookupResult in recordKeyLookupResults)
            {
                try
                {
                    var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!supportingItemsGuidCollection.ContainsKey(splitKeys[2]))
                    {
                        supportingItemsGuidCollection.Add(splitKeys[2], recordKeyLookupResult.Value.Guid);
                    }
                }
                catch (Exception)
                {
                    repositoryErrors.AddError(new RepositoryError("Bad.Data", string.Format("Error occurred while getting guids for MAILING lookup using '{0}'.", recordKeyLookupResult.Key.Split('+')[2]))
                    {
                        SourceId = recordKeyLookupResult.Key.Split('+').Count() > 1 ? recordKeyLookupResult.Key.Split('+')[1] : recordKeyLookupResult.Key
                    });
                }
            }

            return supportingItemsGuidCollection;
        }

        /// <summary>
        /// Return a Person ID from an Application ID
        /// </summary>
        /// <param name="id">Applications Record Key</param>
        /// <returns>Person ID from application</returns>
        public async Task<string> GetPersonIdFromApplicationIdAsync(string id)
        {
            var criteria = "SAVING APPL.APPLICANT";
            var ids = new List<string>() { id };
            var applicantId = await DataReader.SelectAsync("APPLICATIONS",ids.ToArray(), criteria);
            if (applicantId != null && applicantId.Any())
            {
                return applicantId.ElementAt(0).ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// Return an applications Key from the GUID
        /// </summary>
        /// <param name="guid">GUID</param>
        /// <returns>Returns application record key from a GUID</returns>
        public async Task<string> GetApplicationIdFromGuidAsync(string guid)
        {
            var admissionApplicationId = await GetIdFromGuidAsync(guid, "APPLICATIONS");
            if (string.IsNullOrEmpty(admissionApplicationId))
            {
                throw new KeyNotFoundException("Admission applications key not found for GUID " + guid + ".");
            }

            Collection<ApplicationStatuses> categoryTable = await DataReader.BulkReadRecordAsync<ApplicationStatuses>("WITH APPS.SPECIAL.PROCESSING.CODE EQ ''");

            var admissionApplicationDataContract = await DataReader.ReadRecordAsync<Applications>("APPLICATIONS", admissionApplicationId);
            if (admissionApplicationDataContract == null)
            {
                throw new KeyNotFoundException("Admission applications not found for Id " + admissionApplicationId + ".");
            }

            if (categoryTable != null && categoryTable.Any())
            {
                var statuses = categoryTable.Select(rk => rk.Recordkey).Distinct().ToArray();

                if (admissionApplicationDataContract.ApplStatusesEntityAssociation != null && admissionApplicationDataContract.ApplStatusesEntityAssociation.Any())
                {
                    var admApplStatuses = admissionApplicationDataContract.ApplStatusesEntityAssociation.FirstOrDefault();

                    if (statuses.Contains(admApplStatuses.ApplStatusAssocMember))
                    {
                        throw new KeyNotFoundException("Admission applications not found for GUID " + guid + ".");
                    }
                }
            }
            return admissionApplicationId;
        }

        /// <summary>
        /// Return a Key from the GUID
        /// </summary>
        /// <param name="id">GUID</param>
        /// <returns>Returns record key from a GUID</returns>
        public async Task<string> GetIdFromGuidAsync(string id, string entity = "", string secondaryField = "")
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(id) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("AdmissionApplicationSupportingItems GUID " + id + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("AdmissionApplicationSupportingItems GUID " + id + " lookup failed.");
            }

            if (!string.IsNullOrEmpty(entity) && foundEntry.Value.Entity != entity)
            {
                throw new RepositoryException("GUID " + id + " has different entity, " + foundEntry.Value.Entity + ", than expected, " + entity);
            }
            if (!string.IsNullOrEmpty(secondaryField) && string.IsNullOrEmpty(foundEntry.Value.SecondaryKey))
            {
                throw new RepositoryException("GUID " + id + " has no secondary field value for " + secondaryField);
            }
            if (!string.IsNullOrEmpty(secondaryField) && !string.IsNullOrEmpty(foundEntry.Value.SecondaryKey))
            {
                return foundEntry.Value.SecondaryKey;
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Get a single AdmissionApplicationSupportingItem using a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>AdmissionApplicationSupportingItem</returns>
        public async Task<Domain.Student.Entities.AdmissionApplicationSupportingItem> GetAdmissionApplicationSupportingItemsByGuidAsync(string guid)
        {
            var admissionApplicationSupportingItemId = await GetAdmissionApplicationSupportingItemsIdFromGuidAsync(guid);
            var primaryKey = admissionApplicationSupportingItemId.Value.PrimaryKey;
            var secondaryKey = admissionApplicationSupportingItemId.Value.SecondaryKey;
            if (string.IsNullOrEmpty(admissionApplicationSupportingItemId.Value.SecondaryKey))
            {
                throw new KeyNotFoundException(string.Format("AdmissionApplicationSupportingItem id not found for guid {0}" , guid));
            }
            var corresReceivedId = secondaryKey.Split('*');
            var applicationId = corresReceivedId[0] != null ? corresReceivedId[0] : string.Empty;
            var corresReceivedCode = corresReceivedId[1] != null ? corresReceivedId[1] : string.Empty;
            var corresReceivedAssignDate = corresReceivedId[2] != null ? corresReceivedId[2] : string.Empty;
            var corresReceivedInstance = corresReceivedId[3] != null ? corresReceivedId[3] : string.Empty;
            for (var index = 4; index < corresReceivedId.Count(); index++)
            {
                corresReceivedInstance = string.Concat(corresReceivedInstance, "*", corresReceivedId[index]);
            }
            ////convert a unidata internal date valut to datetime
            DateTime? corresReceivedDate = null;
            if (!string.IsNullOrEmpty(corresReceivedAssignDate))
                corresReceivedDate = DmiString.PickDateToDateTime(Convert.ToInt16(corresReceivedAssignDate));

            var admissionApplicationSupportingItem = await GetAdmissionApplicationSupportingItemsByIdAsync(guid, applicationId, corresReceivedCode, corresReceivedDate, corresReceivedInstance);

            return admissionApplicationSupportingItem;
        }

        /// <summary>
        /// Get a single AdmissionApplicationSupportingItems domain entity from an id.
        /// </summary>
        /// <param name="id">The id</param>
        /// <returns>AdmissionApplicationSupportingItems domain entity object</returns>
        public async Task<Domain.Student.Entities.AdmissionApplicationSupportingItem> GetAdmissionApplicationSupportingItemsByIdAsync(string guid, string id, string corresReceivedCode, DateTime? CorresReceivedAssignDate, string CorresReceivedInstance)
        {
             if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a AdmissionApplicationSupportingItems.");
            }
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "guid is required to get a AdmissionApplicationSupportingItems.");
            }
            var applicationDataContract = await DataReader.ReadRecordAsync<Applications>(id);
            if (applicationDataContract == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or Application record with ID ", id, "invalid."));
            }

            var mailingId = applicationDataContract.ApplApplicant;
            var mailingDataContract = await DataReader.ReadRecordAsync<Mailing>(mailingId);
            if (mailingDataContract == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or Mailing record with ID '", mailingId, "' invalid."));
            }
            var mailingCorrReceived = mailingDataContract.ChCorrEntityAssociation.FirstOrDefault(
                c => c.MailingCorrReceivedAssocMember == corresReceivedCode &&
                c.MailingCorrRecvdAsgnDtAssocMember == CorresReceivedAssignDate &&
                c.MailingCorrRecvdInstanceAssocMember == CorresReceivedInstance);
            // Make sure that the association still exists for this supporting item.
            if (mailingCorrReceived == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or Mailing record with ID '", mailingId, "' invalid."));
            }
            // Make sure that this supporting item is still valid and is contained
            // within the mailing record index values field.
            var indexValue = string.Concat(id, "*", corresReceivedCode, "*", DmiString.DateTimeToPickDate(Convert.ToDateTime(CorresReceivedAssignDate)), "*", CorresReceivedInstance);
            if (!mailingDataContract.MailingAdmAppSiIdx.Contains(indexValue))
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or Mailing record with ID '", mailingId, "' invalid."));
            }

            CcComments commentData = null;
            if (mailingCorrReceived != null && !string.IsNullOrEmpty(mailingCorrReceived.MailingCorrRecvdCommentAssocMember))
            {
                commentData = await DataReader.ReadRecordAsync<CcComments>("CC.COMMENTS", mailingCorrReceived.MailingCorrRecvdCommentAssocMember);
            }
            List<Coreq> coreqData = null;
            List<string> coreqIds = new List<string>();
            if (mailingDataContract != null)
            {
                foreach (var cc in mailingDataContract.MailingCurrentCrcCode)
                {
                    coreqIds.Add(string.Concat(mailingDataContract.Recordkey, '*', cc));
                }
            }
            if (coreqIds != null && coreqIds.Any())
            {
                coreqData = (await DataReader.BulkReadRecordAsync<Coreq>("COREQ", coreqIds.ToArray())).ToList();
            }

            var supportingItemDto = await BuildAdmissionApplicationSupportingItem(guid, mailingId, applicationDataContract.RecordGuid, mailingCorrReceived, commentData, coreqData);

            if (repositoryErrors.Errors != null && repositoryErrors.Errors.Any())
            {
                throw repositoryErrors;
            }
            return supportingItemDto;
        }

        /// <summary>
        ///  Get all AdmissionApplicationSupportingItems domain entity data 
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <param name="offset"></param>
        /// <returns>Collection of AdmissionApplicationSupportingItems domain entities</returns>
        public async Task<Tuple<IEnumerable<Domain.Student.Entities.AdmissionApplicationSupportingItem>, int>> GetAdmissionApplicationSupportingItemsAsync(int offset, int limit, bool bypassCache = false)
        {
            string[] suppItemIds = null;
            string criteria = string.Empty;
            int totalCount = 0;
            string[] subList = null;

            string studentAcademicProgramsCacheKey = CacheSupport.BuildCacheKey(suppItemsKeysCacheKey);

            var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                this,
                ContainsKey,
                GetOrAddToCacheAsync,
                AddOrUpdateCacheAsync,
                transactionInvoker,
                studentAcademicProgramsCacheKey,
                "",
                offset,
                limit,
                suppItemsCacheTimeout,
                async () =>
                {
                    var applStatusesNoSpCodeIds = await DataReader.SelectAsync("APPLICATION.STATUSES", "WITH APPS.SPECIAL.PROCESSING.CODE NE ''");

                    var paragraph = new List<string>();
                    
                    paragraph.Add("MIOSEL APPLICATIONS WITH APPL.STATUS.DATE NE '' AND WITH APPL.STATUS.TIME NE '' AND WITH APPL.STATUS = '" 
                            + (string.Join(" ", applStatusesNoSpCodeIds.Distinct())).Replace(" ", "' '") + "' AND WITH APPL.APPLICANT NE '' SAVING UNIQUE APPL.APPLICANT");
                    
                    paragraph.Add("MIOSEL MAILING WITH MAILING.ADM.APP.SI.IDX NE '' BY.EXP MAILING.ADM.APP.SI.IDX SAVING MAILING.ADM.APP.SI.IDX REQUIRE.SELECT");

                    return new CacheSupport.KeyCacheRequirements()
                    {
                        paragraph = paragraph,
                    };
                }
            );

            if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
            {
                return new Tuple<IEnumerable<Domain.Student.Entities.AdmissionApplicationSupportingItem>, int>(null, 0);
            }

            subList = keyCache.Sublist.ToArray();
            totalCount = keyCache.TotalCount.Value;

            // In Unidata, we get the mailing ID, @VM, supporting Item Idx
            var validSupportingItemIds = new List<string>();
            foreach (var supportingItemId in subList)
            {
                var supportingItemKey = supportingItemId;
                if (supportingItemId.Contains(_VM))
                    supportingItemKey = supportingItemId.Split(_VM)[1];
                validSupportingItemIds.Add(supportingItemKey);
            }

            var applIds = validSupportingItemIds.Select(ab => ab.Split('*')[0]).Distinct().ToArray();
            if (!applIds.Any())
            {
                return new Tuple<IEnumerable<Domain.Student.Entities.AdmissionApplicationSupportingItem>, int>(null, 0);
            }
            criteria = "WITH APPL.APPLICANT NE '' SAVING APPL.APPLICANT";
            var mailingSublist = await DataReader.SelectAsync("APPLICATIONS", applIds, criteria);
            if (mailingSublist == null || !mailingSublist.Any())
            {
                return new Tuple<IEnumerable<Domain.Student.Entities.AdmissionApplicationSupportingItem>, int>(null, 0);
            }

            var applicationsData = await DataReader.BulkReadRecordAsync<Applications>("APPLICATIONS", applIds);
            var mailingData = await DataReader.BulkReadRecordAsync<Mailing>("MAILING", mailingSublist.Distinct().ToArray());
            var commentIds = mailingData.SelectMany(md => md.MailingCorrRecvdComment).Distinct().ToArray();
            var commentData = await DataReader.BulkReadRecordAsync<CcComments>("CC.COMMENTS", commentIds);
            List<string> coreqIds = new List<string>();
            if (mailingData != null && mailingData.Any())
            {
                mailingData.ToList().ForEach(
                    md =>
                    {
                        foreach (var cc in md.MailingCurrentCrcCode)
                        {
                            coreqIds.Add(string.Concat(md.Recordkey, '*', cc));
                        }
                    });
            }
            var coreqData = await DataReader.BulkReadRecordAsync<Coreq>("COREQ", coreqIds.ToArray());
            var applSupportingItemsEntities = await BuildAdmissionApplicationSupportingItems(subList.ToList(), applicationsData, mailingData, commentData, coreqData);
            return new Tuple<IEnumerable<Domain.Student.Entities.AdmissionApplicationSupportingItem>, int>(applSupportingItemsEntities, totalCount);
        }

        private async Task<IEnumerable<Domain.Student.Entities.AdmissionApplicationSupportingItem>> BuildAdmissionApplicationSupportingItems(List<string> applItemIds,
            Collection<Applications> applicationsData, Collection<Mailing> mailingData,
            Collection<CcComments> commentData, Collection<Coreq> coreqData)
        {
            var admissionApplicationSupportingItemCollection = new List<Domain.Student.Entities.AdmissionApplicationSupportingItem>();

            var supportingItemsDict = await GetAdmissionApplicationSupportingItemsGuidsCollectionAsync(applItemIds, applicationsData);
            foreach (var supportingItemId in applItemIds)
            {
                string guid = string.Empty;
                supportingItemsDict.TryGetValue(supportingItemId, out guid);

                var corresReceivedId = supportingItemId.Split('*');
                var applicationId = corresReceivedId[0] != null ? corresReceivedId[0] : string.Empty;
                var corresReceivedCode = corresReceivedId[1] != null ? corresReceivedId[1] : string.Empty;
                var corresReceivedAssignDate = corresReceivedId[2] != null ? corresReceivedId[2] : string.Empty;
                var corresReceivedInstance = corresReceivedId[3] != null ? corresReceivedId[3] : string.Empty;
                for (var index = 4; index < corresReceivedId.Count(); index++)
                {
                    corresReceivedInstance = string.Concat(corresReceivedInstance, "*", corresReceivedId[index]);
                }
                ////convert a unidata internal date valut to datetime
                DateTime? corresReceivedDate = null;
                if (!string.IsNullOrEmpty(corresReceivedAssignDate))
                    corresReceivedDate = DmiString.PickDateToDateTime(Convert.ToInt16(corresReceivedAssignDate));
                var application = applicationsData.Where(a => a.Recordkey == applicationId).FirstOrDefault();
                if (application == null)
                {
                    repositoryErrors.AddError(new RepositoryError("Bad.Data", "Mailing index '" + supportingItemId + "' is referencing an invalid application.")
                    {
                        Id = guid,
                        SourceId = application.ApplApplicant
                    });  
                }
                else
                {
                    var mailing = mailingData.Where(m => m.Recordkey == application.ApplApplicant).FirstOrDefault();
                    if (mailing == null)
                    {
                        repositoryErrors.AddError(new RepositoryError("Bad.Data", "Mailing index '" + supportingItemId + "' is referencing an application with an invalid applicant.")
                        {
                            Id = guid,
                            SourceId = application.ApplApplicant
                        });
                    }
                    else
                    {
                        List<Coreq> coreqRecords = new List<Coreq>();
                        foreach (var cr in mailing.MailingCurrentCrcCode)
                        {
                            var coreqs = coreqData.Where(cd => cd.Recordkey == string.Concat(mailing.Recordkey, '*', cr)).ToList();
                            if (coreqs != null && coreqs.Any())
                            {
                                coreqRecords.AddRange(coreqs);
                            }
                        }
                        var corresReceived = mailing.ChCorrEntityAssociation.FirstOrDefault(
                            c => c.MailingCorrReceivedAssocMember == corresReceivedCode &&
                            c.MailingCorrRecvdAsgnDtAssocMember == corresReceivedDate &&
                            c.MailingCorrRecvdInstanceAssocMember == corresReceivedInstance);
                        if (corresReceived == null)
                        {
                            repositoryErrors.AddError(new RepositoryError("Bad.Data", "Mailing index '" + supportingItemId + "' is referencing an invalid application supporting item.")
                            {
                                Id = guid,
                                SourceId = mailing.Recordkey
                            });
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(guid))
                            {
                                var comment = commentData.FirstOrDefault(cd => cd.Recordkey == corresReceived.MailingCorrRecvdCommentAssocMember);
                                try
                                {
                                    var supportingItem = await BuildAdmissionApplicationSupportingItem(guid, mailing.Recordkey, application.RecordGuid, corresReceived, comment, coreqRecords);
                                    if (supportingItem == null)
                                    {
                                        repositoryErrors.AddError(new RepositoryError("Bad.Data", "Mailing index '" + supportingItemId + "' is referencing an invalid application supporting item.")
                                        {
                                            Id = guid,
                                            SourceId = mailing.Recordkey
                                        });
                                    }
                                    else
                                    {
                                        admissionApplicationSupportingItemCollection.Add(supportingItem);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    repositoryErrors.AddError(new RepositoryError("Bad.Data", ex.Message)
                                    {
                                        Id = guid,
                                        SourceId = mailing.Recordkey
                                    });
                                }
                            }
                        }
                    }
                }
            }
            if (repositoryErrors.Errors != null && repositoryErrors.Errors.Any())
            {
                throw repositoryErrors;
            }
            return admissionApplicationSupportingItemCollection.AsEnumerable();
        }

        private async Task<Domain.Student.Entities.AdmissionApplicationSupportingItem> BuildAdmissionApplicationSupportingItem(string guid, string id, string applicationId, MailingChCorr corresReceived, CcComments commentData, List<Coreq> coreqData)
        {
            Domain.Student.Entities.AdmissionApplicationSupportingItem admissionApplicationSupportingItem = null;
            if (corresReceived == null)
            {
                throw new ArgumentNullException("corresReceived", "BuildAdmissionApplicationSupportingItem called with null paramter. ");
            }
            if (!string.IsNullOrEmpty(guid) &&
                corresReceived.MailingCorrRecvdAsgnDtAssocMember.HasValue &&
                !string.IsNullOrEmpty(corresReceived.MailingCorrReceivedAssocMember))
            {
                try
                {
                    admissionApplicationSupportingItem = new Domain.Student.Entities.AdmissionApplicationSupportingItem(guid,
                        id,
                        applicationId,
                        corresReceived.MailingCorrReceivedAssocMember,
                        corresReceived.MailingCorrRecvdInstanceAssocMember,
                        corresReceived.MailingCorrRecvdAsgnDtAssocMember,
                        corresReceived.MailingCorrRecvdStatusAssocMember);
                }
                catch (ArgumentNullException)
                {
                    repositoryErrors.AddError(new RepositoryError("Bad.Data", string.Format("Null parameters found when creating Entity for guid '{0}', person '{1}', application '{2}'", guid, id, applicationId))
                    {
                        Id = guid,
                        SourceId = id
                    });
                }

                admissionApplicationSupportingItem.ActionDate = corresReceived.MailingCorrRecvdActDtAssocMember;
                admissionApplicationSupportingItem.ReceivedDate = corresReceived.MailingCorrReceivedDateAssocMember;
                var valAssoc = (await GetCorrStatuses()).ValsEntityAssociation.FirstOrDefault(vea =>
                    vea.ValInternalCodeAssocMember == corresReceived.MailingCorrRecvdStatusAssocMember);
                if (valAssoc != null)
                {
                    admissionApplicationSupportingItem.StatusAction = valAssoc.ValActionCode1AssocMember;
                }
                // Comments from CC.COMMENTS
                if (commentData != null)
                {
                    var comments = commentData.CcCommentsText.Replace(_VM, ' ');
                    admissionApplicationSupportingItem.Comment = comments;
                }
                // Required Flag
                if (coreqData != null)
                {
                    foreach (var coreq in coreqData)
                    {
                        foreach (var ccCode in coreq.CoreqRequestsEntityAssociation)
                        {
                            if (ccCode != null && ccCode.CoreqCcCodeAssocMember != null)
                            {
                                if (ccCode.CoreqCcCodeAssocMember.Equals(corresReceived.MailingCorrReceivedAssocMember, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (!string.IsNullOrEmpty(ccCode.CoreqCcRequiredAssocMember) && ccCode.CoreqCcRequiredAssocMember.ToUpper() == "Y")
                                    {
                                        admissionApplicationSupportingItem.Required = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return admissionApplicationSupportingItem;
        }

        /// <summary>
        /// Update an existing admissions application supporting item
        /// </summary>
        /// <param name="admissionApplictaionSupportingItem">AdmissionApplicationSupportingItem Entity</param>
        /// <returns>Domain Entity AdmissionApplicationSupportingItem</returns>
        public async Task<Domain.Student.Entities.AdmissionApplicationSupportingItem> UpdateAdmissionApplicationSupportingItemsAsync(Domain.Student.Entities.AdmissionApplicationSupportingItem admissionApplictaionSupportingItem)
        {
            if (admissionApplictaionSupportingItem == null)
                throw new ArgumentNullException("admissionApplicationSupportingItemEntity", "Must provide a admissionApplicationSupportingItemEntityEntity to update.");
            if (string.IsNullOrEmpty(admissionApplictaionSupportingItem.Guid))
                throw new ArgumentNullException("admissionApplicationSupportingItemEntity", "Must provide the guid of the admissionApplicationSupportingItemEntityEntity to update.");

            // verify the GUID exists to perform an update.  If not, perform a create instead
            var lookupResult = await GetAdmissionApplicationSupportingItemsIdFromGuidAsync(admissionApplictaionSupportingItem.Guid);

            if (lookupResult.Value != null &&  !string.IsNullOrEmpty(lookupResult.Value.SecondaryKey))
            {
                var updateRequest = BuildUpdateRequest(admissionApplictaionSupportingItem);

                // write the  data
                var updateResponse = await transactionInvoker.ExecuteAsync<UpdateApplicationSupportingItemRequest, UpdateApplicationSupportingItemResponse>(updateRequest);

                if (updateResponse.UpdateAdmApplSupportingItemsErrors.Any())
                {
                    var errorMessage = string.Format("Error(s) occurred updating admission application supporting item '{0}':", admissionApplictaionSupportingItem.Guid);
                    updateResponse.UpdateAdmApplSupportingItemsErrors.ForEach(e => repositoryErrors.AddError(new RepositoryError("Bad.Data", string.Concat(string.IsNullOrEmpty(e.ErrorCodes) ? "" : e.ErrorCodes + ": ", e.ErrorMessages))
                    {
                        Id = admissionApplictaionSupportingItem.Guid,
                        SourceId = admissionApplictaionSupportingItem.MailingId
                    }));
                    logger.Error(errorMessage);
                    throw repositoryErrors;
                }

                // get the updated entity from the database
                return await GetAdmissionApplicationSupportingItemsByGuidAsync(admissionApplictaionSupportingItem.Guid);
            }

            // perform a create instead
            return await CreateAdmissionApplicationSupportingItemsAsync(admissionApplictaionSupportingItem);
        }

        /// <summary>
        /// Create a new admission application supporting item
        /// </summary>
        /// <param name="admissionApplictaionSupportingItem">AdmissionApplicationSupportingItem Entity</param>
        /// <returns>Domain entity AdmissionApplicationSupportingItem</returns>
        public async Task<Domain.Student.Entities.AdmissionApplicationSupportingItem> CreateAdmissionApplicationSupportingItemsAsync(Domain.Student.Entities.AdmissionApplicationSupportingItem admissionApplictaionSupportingItem)
        {
            if (admissionApplictaionSupportingItem == null)
                throw new ArgumentNullException("admissionApplicationSupportingItemEntity", "Must provide a admissionApplicationSupportingItemEntity to create.");

            var createRequest = BuildUpdateRequest(admissionApplictaionSupportingItem);
            // write the  data
            var createResponse = await transactionInvoker.ExecuteAsync<UpdateApplicationSupportingItemRequest, UpdateApplicationSupportingItemResponse>(createRequest);

            if (createResponse.UpdateAdmApplSupportingItemsErrors.Any())
            {
                var errorMessage = string.Format("Error(s) occurred creating new admission application supporting item for person '{0}', application '{1}', item type '{2}': ",createResponse.PersonId, admissionApplictaionSupportingItem.ApplicationId, admissionApplictaionSupportingItem.ReceivedCode);
                var exception = new RepositoryException(errorMessage);
                createResponse.UpdateAdmApplSupportingItemsErrors.ForEach(e => exception.AddError(new RepositoryError(string.IsNullOrEmpty(e.ErrorCodes) ? "" : e.ErrorCodes, e.ErrorMessages)));
                logger.Error(errorMessage);
                throw exception;
            }

            // get the newly created entity from the database
            return await GetAdmissionApplicationSupportingItemsByGuidAsync(createResponse.Guid);
        }

        /// <summary>
        /// Create an ApplicationSupportingItemRequest from a domain entity
        /// </summary>
        /// <param name="admissionApplicationSupportingItemEntity">supporting item domain entity</param>
        /// <returns>UpdateApplicationSupportingItemRequest transaction object</returns>
        private UpdateApplicationSupportingItemRequest BuildUpdateRequest(Domain.Student.Entities.AdmissionApplicationSupportingItem admissionApplicationSupportingItemEntity)
        {
            var request = new UpdateApplicationSupportingItemRequest()
            {
                Guid = admissionApplicationSupportingItemEntity.Guid,
                PersonId = admissionApplicationSupportingItemEntity.MailingId,
                ApplicationId = admissionApplicationSupportingItemEntity.ApplicationId,
                CorrespondenceType = admissionApplicationSupportingItemEntity.ReceivedCode,
                CorrespondenceAssignDate = admissionApplicationSupportingItemEntity.AssignedDate,
                CorrespondenceRequired = admissionApplicationSupportingItemEntity.Required ? "Y" : "N",
                CorrespondenceInstanceName = admissionApplicationSupportingItemEntity.Instance,
                CorrespondenceStatus = admissionApplicationSupportingItemEntity.Status,
                CorrespondenceActionDate = admissionApplicationSupportingItemEntity.ActionDate,
                CorrespondenceReceivedDate = !string.IsNullOrEmpty(admissionApplicationSupportingItemEntity.Status) ? admissionApplicationSupportingItemEntity.ReceivedDate : null,
                CorrespondenceComment = admissionApplicationSupportingItemEntity.Comment
            };

            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }

            return request;
        }

        /// <summary>
        /// Return the Validation Table InstTypes for determination of High School or College
        /// within the Institutions Attended data.
        /// </summary>
        /// <returns>Validation Table Object for Institution Types</returns>
        private async Task<ApplValcodes> GetCorrStatuses()
        {
            if (_corrStatuses != null)
            {
                return _corrStatuses;
            }

            _corrStatuses = await GetOrAddToCacheAsync<ApplValcodes>("AllCorrespondenceReceivedStatuses",
                async () =>
                {
                    ApplValcodes typesTable = await DataReader.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "CORR.STATUSES");
                    if (typesTable == null)
                    {
                        var errorMessage = "Unable to access CORR.STATUSES valcode table.";
                        logger.Info(errorMessage);
                        throw new Exception(errorMessage);
                    }
                    return typesTable;
                }, Level1CacheTimeoutValue);
            return _corrStatuses;
        }

    }
}