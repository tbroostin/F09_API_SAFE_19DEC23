// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Repository for attachment data.
    /// </summary>
    [RegisterType]
    public class AttachmentRepository : BaseColleagueRepository, IAttachmentRepository
    {
        private readonly string colleagueTimeZone;
        private IntlParams internationalParameters;

        private const string attachmentActiveStatus = "A";
        private const string attachmentDeletedStatus = "D";
        private const string attachmentContentDir = "ATTACHMENT.CONTENT";

        private static string attachmentContentDirPath = null;
        private static string appServerOSPathSeparator = null;
        private static string attachmentTempFilePath = null;

        /// <summary>
        /// Constructor for AttachmentRepository
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="transactionFactory">The transaction factory.</param>
        /// <param name="logger">The logger.</param>
        public AttachmentRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger,
            ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            colleagueTimeZone = settings.ColleagueTimeZone;
        }

        /// <summary>
        /// Get the attachment temp file path on the server
        /// </summary>
        /// <returns>The attachment temp file path on the server</returns>
        public string GetAttachTempFilePath()
        {
            if (string.IsNullOrEmpty(attachmentTempFilePath))
            {
                attachmentTempFilePath = Path.Combine(HttpRuntime.AppDomainAppPath, "App_Data", "Temp");
            }
            return attachmentTempFilePath;
        }

        /// <summary>
        /// Get attachments
        /// </summary>
        /// <param name="owner">Owner to get attachments for</param>
        /// <param name="collectionId">Collection Id to get attachments for</param>
        /// <param name="tagOne">TagOne value to get attachments for</param>
        /// <returns>List of <see cref="Attachment">Attachments</see></returns>
        public async Task<IEnumerable<Attachment>> GetAttachmentsAsync(string owner, string collectionId, string tagOne)
        {
            // owner or collection ID is required to continue
            if (string.IsNullOrEmpty(owner) && string.IsNullOrEmpty(collectionId))
                throw new ArgumentException("Required filter not provided to get attachments");

            // get attachments
            var criteria = new StringBuilder();
            // owner
            if (!string.IsNullOrEmpty(owner))
            {
                criteria.Append(string.Format("WITH ATT.OWNER EQ '{0}'", owner));
            }
            // collection ID
            if (!string.IsNullOrEmpty(collectionId))
            {
                if (criteria.Length > 0)
                    criteria.Append(string.Format(" AND ATT.COLLECTION.ID EQ '{0}'", collectionId));
                else
                    criteria.Append(string.Format("WITH ATT.COLLECTION.ID EQ '{0}'", collectionId));
            }
            // tagOne
            if (!string.IsNullOrEmpty(tagOne))
            {
                criteria.Append(string.Format(" AND ATT.TAG.ONE EQ '{0}'", tagOne));
            }
            criteria.Append(string.Format(" AND ATT.STATUS EQ '{0}'", attachmentActiveStatus));

            // do not include the encryption metadata
            try
            {
                var attachmentRecords = await DataReader.BulkReadRecordAsync<AttachmentsNoEncr>(criteria.ToString());
                return BuildAttachments(attachmentRecords);
            }
            catch (Exception e)
            {
                string error = "Error occurred retrieving bulk attachment metadata";
                logger.Error(e, error);
                throw new RepositoryException(error);
            }            
        }

        /// <summary>
        /// Get the attachment by ID
        /// </summary>
        /// <param name="attachmentId">ID of the attachment</param>
        /// <returns>The <see cref="Attachment">attachment</see></returns>
        public async Task<Attachment> GetAttachmentByIdWithEncrMetadataAsync(string attachmentId)
        {
            if (string.IsNullOrEmpty(attachmentId))
                throw new ArgumentNullException("attachmentId");

            // get the attachment by ID, including encryption metadata
            try
            {
                var attachmentRecord = await DataReader.ReadRecordAsync<Attachments>(attachmentId);
                return BuildAttachments(new List<Attachments>() { attachmentRecord }).FirstOrDefault();
            }
            catch (Exception e)
            {
                string error = "Error occurred retrieving attachment metadata";
                logger.Error(e, string.Format("{0} for attachment ID {1}", error, attachmentId));
                throw new RepositoryException(error);
            }        
        }

        /// <summary>
        /// Get the attachment by ID and do not include its encryption parameters
        /// </summary>
        /// <param name="attachmentId">ID of the attachment</param>
        /// <returns>The <see cref="Attachment">attachment</see></returns>
        public async Task<Attachment> GetAttachmentByIdNoEncrMetadataAsync(string attachmentId)
        {
            if (string.IsNullOrEmpty(attachmentId))
                throw new ArgumentNullException("attachmentId");

            // get the attachment by ID w/ no encryption metadata
            try
            {
                var attachmentRecord = await DataReader.ReadRecordAsync<AttachmentsNoEncr>(attachmentId);
                return BuildAttachments(new List<AttachmentsNoEncr>() { attachmentRecord }).FirstOrDefault();
            }
            catch (Exception e)
            {
                string error = "Error occurred retrieving attachment metadata";
                logger.Error(e, string.Format("{0} for attachment ID {1}", error, attachmentId));
                throw new RepositoryException(error);
            }
        }

        /// <summary>
        /// Get the attachment's content
        /// </summary>
        /// <param name="attachment">The attachment</param>
        /// <returns>Path to the attachment's temp file location</returns>
        public async Task<string> GetAttachmentContentAsync(Attachment attachment)
        {
            if (attachment == null)
                throw new ArgumentNullException("attachment");

            if (attachmentContentDirPath == null)
            {
                attachmentContentDirPath = await GetApphomePathAsync() 
                    + await GetAppServerOSPathSeparatorAsync() 
                    + attachmentContentDir;
            }
            if (appServerOSPathSeparator == null)
            {
                appServerOSPathSeparator = await GetAppServerOSPathSeparatorAsync();
            }
            var attachmentContentPath = attachmentContentDirPath
                    + appServerOSPathSeparator
                    + attachment.Id;

            // setup the temp path and delete the temp file if it exists.  Use a different file name
            string tempAttachmentPath = Path.Combine(GetAttachTempFilePath(), Guid.NewGuid().ToString());
            if (File.Exists(tempAttachmentPath))
                File.Delete(tempAttachmentPath);

            try
            {
                await DmiFileTransferClient.SendDownloadRequestAsync(attachmentContentPath, tempAttachmentPath).ConfigureAwait(false);
                return tempAttachmentPath;
            }
            catch (Exception e)
            {
                // error occurred, delete the temp file
                if (File.Exists(tempAttachmentPath))
                {
                    try
                    {
                        File.Delete(tempAttachmentPath);
                    }
                    catch (Exception fde)
                    {
                        logger.Info(fde, string.Format("Could not delete temp file {0}", tempAttachmentPath));
                    }
                }
                string error = "Error occurred retrieving attachment content";
                logger.Error(e, error);
                throw new RepositoryException(error);
            }
        }

        /// <summary>
        /// Query attachments
        /// </summary>
        /// <param name="includeActiveOnly">True if only attachments with an active status must be returned</param>
        /// <param name="owner">The attachment owner to query by</param>
        /// <param name="modifyStartDate">The start of the attachment modified date range to query by</param>
        /// <param name="modifyEndDate">The end of the attachment modified date range to query by</param>
        /// <param name="collectionIds">List of collection IDs to query by</param>
        /// <returns>List of <see cref="Attachment">Attachments</see></returns>
        public async Task<IEnumerable<Attachment>> QueryAttachmentsAsync(bool includeActiveOnly, string owner, DateTime? modifyStartDate,
            DateTime? modifyEndDate, IEnumerable<string> collectionIds)
        {
            if (collectionIds == null || !collectionIds.Any())
                throw new ArgumentException("Collection ID(s) are required to query attachments");
            if (!modifyStartDate.HasValue || !modifyEndDate.HasValue)
                throw new ArgumentException("Start and end date are required to query attachments");
            if (DateTime.Compare(modifyStartDate.Value, modifyEndDate.Value) > 0)
                throw new ArgumentOutOfRangeException("modifyStartDate", "Attachment query start date cannot be later than end date");

            var criteria = new StringBuilder();
            // owner
            if (!string.IsNullOrEmpty(owner))
            {
                criteria.Append(string.Format("WITH (ATT.OWNER EQ '{0}')", owner));
            }
            // collection ID(s)
            var collectionIdCriteria = new StringBuilder();
            foreach (var collectionId in collectionIds)
            {
                if (!string.IsNullOrEmpty(collectionId))
                {
                    if (collectionIdCriteria.Length > 0)
                        collectionIdCriteria.Append(string.Format(" OR ATT.COLLECTION.ID EQ '{0}'", collectionId));
                    else
                        collectionIdCriteria.Append(string.Format(" WITH (ATT.COLLECTION.ID EQ '{0}'", collectionId));
                }
            }
            if (collectionIdCriteria.Length > 0)
            {
                collectionIdCriteria.Append(")");
                criteria.Append(collectionIdCriteria.ToString());
            }
            else
            {
                throw new ArgumentException("No valid Collection ID(s) provided to query attachments");
            }
            // date range
            var internationalParameters = GetInternationalParameters();
            var formattedStartDate = UniDataFormatter.UnidataFormatDate(modifyStartDate.Value, internationalParameters.HostShortDateFormat,
                internationalParameters.HostDateDelimiter);
            var formattedEndDate = UniDataFormatter.UnidataFormatDate(modifyEndDate.Value, internationalParameters.HostShortDateFormat,
                internationalParameters.HostDateDelimiter);
            criteria.Append(string.Format(" AND (ATTACHMENTS.CHGDATE GE '{0}' AND ATTACHMENTS.CHGDATE LE '{1}')",
                formattedStartDate, formattedEndDate));
            // active attachments only
            if (includeActiveOnly)
            {
                criteria.Append(string.Format(" AND (ATT.STATUS EQ '{0}')", attachmentActiveStatus));
            }

            // do not include the encryption metadata
            try
            {
                var attachmentRecords = await DataReader.BulkReadRecordAsync<AttachmentsNoEncr>(criteria.ToString());
                return BuildAttachments(attachmentRecords);
            }
            catch (Exception e)
            {
                string error = "Error occurred retrieving attachment metadata by query";
                logger.Error(e, error);
                throw new RepositoryException(error);
            }
        }

        // get the date format
        private IntlParams GetInternationalParameters()
        {
            if (internationalParameters == null)
            {
                internationalParameters = GetOrAddToCache("InternationalParameters",
                () =>
                {
                    var intlParams = DataReader.ReadRecord<IntlParams>("INTL.PARAMS", "INTERNATIONAL");
                    if (intlParams == null)
                    {
                        logger.Info("Unable to access international parameters INTL.PARAMS INTERNATIONAL.");
                        // default to US with a / delimiter.
                        intlParams = new IntlParams
                        {
                            HostShortDateFormat = "MDY",
                            HostDateDelimiter = "/"
                        };
                    }
                    return intlParams;
                }, Level1CacheTimeoutValue);
            }

            return internationalParameters;
        }

        /// <summary>
        /// Create the new attachment
        /// </summary>
        /// <param name="attachment">The <see cref="Attachment">Attachment</see> to create</param>
        /// <returns>Newly created <see cref="Attachment">Attachment</see></returns>
        public async Task<Attachment> PostAttachmentAsync(Attachment attachment)
        {
            if (attachment == null)
                throw new ArgumentNullException("attachment");

            var updateRequest = new UpdateAttachmentsRequest()
            {
                Id = attachment.Id,
                CollectionId = attachment.CollectionId,
                ContentType = !string.IsNullOrEmpty(attachment.ContentType) ? attachment.ContentType.ToLower() : null,
                EncrKeyId = attachment.EncrKeyId,
                EncrType = attachment.EncrType,
                Name = attachment.Name,
                Owner = attachment.Owner,
                Size = attachment.Size.ToString(),
                Status = ConvertAttachmentStatusToInternal(attachment.Status),
                TagOne = attachment.TagOne
            };

            if (attachment.EncrContentKey != null && attachment.EncrContentKey.Length > 0)
            {
                updateRequest.EncrContentKey = Convert.ToBase64String(attachment.EncrContentKey);
            }
            if (attachment.EncrIV != null && attachment.EncrIV.Length > 0)
            {
                updateRequest.EncrIV = Convert.ToBase64String(attachment.EncrIV);
            }

            try
            {
                // call the CTX to create the attachment
                var createResponse = await transactionInvoker.ExecuteAsync<UpdateAttachmentsRequest, UpdateAttachmentsResponse>(updateRequest);
                if (createResponse == null)
                {
                    string error = "No response received creating attachment";
                    logger.Error(string.Format("{0}, attachment {1} ({2})", error, attachment.Id, attachment.Name));
                    throw new RepositoryException(error);
                }
                if (!string.IsNullOrEmpty(createResponse.ErrorMsg))
                {
                    string error = string.Format("Error occurred creating attachment");
                    logger.Error(string.Format("{0}, attachment {1} ({2}) : error = {3}", error, attachment.Id, attachment.Name, createResponse.ErrorMsg));
                    throw new RepositoryException(error);
                }
            }
            catch (ColleagueTransactionException cte)
            {
                string error = string.Format("Exception occurred creating attachment");
                logger.Error(cte, string.Format("{0}, attachment {1} ({2}) : error = {3}", error, attachment.Id, attachment.Name, cte.Message));
                throw new RepositoryException(error);
            }

            // return the newly created attachment
            return await GetAttachmentByIdNoEncrMetadataAsync(attachment.Id);
        }

        /// <summary>
        /// Create the new attachment w/ content
        /// </summary>
        /// <param name="attachment">The attachment's metadata</param>
        /// <param name="attachmentContentStream">Stream to the attachment content</param>
        /// <returns>Newly created <see cref="Attachment">Attachment</see></returns>
        public async Task<Attachment> PostAttachmentAndContentAsync(Attachment attachment, Stream attachmentContentStream)
        {            
            if (attachment == null)
                throw new ArgumentNullException("attachment");
            if (attachmentContentStream == null)
                throw new ArgumentNullException("attachmentContentStream");

            if (attachmentContentDirPath == null)
            {
                attachmentContentDirPath = await GetApphomePathAsync()
                    + await GetAppServerOSPathSeparatorAsync()
                    + attachmentContentDir;
            }
            if (appServerOSPathSeparator == null)
            {
                appServerOSPathSeparator = await GetAppServerOSPathSeparatorAsync();
            }
            var attachmentContentPath = attachmentContentDirPath
                    + appServerOSPathSeparator
                    + attachment.Id;

            // ensure the stream is at the beginning
            attachmentContentStream.Position = 0;

            // post the attachment metadata
            var attachmentEntity = await PostAttachmentAsync(attachment);

            ExceptionDispatchInfo uploadCapturedException = null;
            // read the stream in chunks, passing them through DMI until the whole stream is processed, then close the stream
            using (attachmentContentStream)
            {
                try
                {
                    await DmiFileTransferClient.SendUploadRequestAsync(
                        attachmentContentPath, attachment.Size.Value, attachmentContentStream).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    uploadCapturedException = ExceptionDispatchInfo.Capture(e);
                }
            }

            if (uploadCapturedException != null)
            {
                Exception e = uploadCapturedException.SourceException;

                // the file upload failed
                logger.Error(e, string.Format("Attachment content upload failed for attachment {0} ({1}) : error = {2}",
                    attachment.Id, attachment.Name, e.Message));

                // rollback the associated attachment metadata
                await RollbackAttachmentAsync(attachment);

                throw new RepositoryException("Attachment content upload failed");
            }

            // return the newly created attachment
            return attachmentEntity;
        }

        /// <summary>
        /// Update the attachment
        /// </summary>
        /// <param name="attachment">The <see cref="Attachment">Attachment</see> to update</param>
        /// <returns>Updated <see cref="Attachment">Attachment</see></returns>
        public async Task<Attachment> PutAttachmentAsync(Attachment attachment)
        {
            if (attachment == null)
                throw new ArgumentNullException("attachment");

            var updateRequest = new UpdateAttachmentsRequest()
            {
                Id = attachment.Id,
                CollectionId = attachment.CollectionId,
                ContentType = !string.IsNullOrEmpty(attachment.ContentType) ? attachment.ContentType.ToLower() : null,
                EncrKeyId = attachment.EncrKeyId,
                EncrType = attachment.EncrType,
                Name = attachment.Name,
                Owner = attachment.Owner,
                Size = attachment.Size.ToString(),
                Status = ConvertAttachmentStatusToInternal(attachment.Status),
                TagOne = attachment.TagOne
            };

            if (attachment.EncrContentKey != null && attachment.EncrContentKey.Length > 0)
            {
                updateRequest.EncrContentKey = Convert.ToBase64String(attachment.EncrContentKey);
            }
            if (attachment.EncrIV != null && attachment.EncrIV.Length > 0)
            {
                updateRequest.EncrIV = Convert.ToBase64String(attachment.EncrIV);
            }

            try
            {
                // call the CTX to update the attachment
                var updateResponse = await transactionInvoker.ExecuteAsync<UpdateAttachmentsRequest, UpdateAttachmentsResponse>(updateRequest);
                if (updateResponse == null)
                {
                    string error = "No response received updating attachment";
                    logger.Error(string.Format("{0}, attachment {1} ({2})", error, attachment.Id, attachment.Name));
                    throw new RepositoryException(error);
                }
                if (!string.IsNullOrEmpty(updateResponse.ErrorMsg))
                {
                    string error = string.Format("Error occurred updating attachment");
                    logger.Error(string.Format("{0}, attachment {1} ({2}) : error = {3}", error, attachment.Id, attachment.Name, updateResponse.ErrorMsg));
                    throw new RepositoryException(error);
                }
            }
            catch (ColleagueTransactionException cte)
            {
                string error = string.Format("Exception occurred updating attachment");
                logger.Error(cte, string.Format("{0}, attachment {1} ({2}) : error = {3}", error, attachment.Id, attachment.Name, cte.Message));
                throw new RepositoryException(error);
            }

            // return the updated attachment
            return await GetAttachmentByIdNoEncrMetadataAsync(attachment.Id);
        }

        /// <summary>
        /// Delete the attachment
        /// </summary>
        /// <param name="attachment">The attachment to delete</param>
        public async Task DeleteAttachmentAsync(Attachment attachment)
        {
            if (attachment == null)
                throw new ArgumentNullException("attachment");

            // setup the delete request
            var deleteRequest = new CrudAttachmentContentRequest()
            {
                Id = attachment.Id,
                Action = "DELETE"
            };

            try
            {
                // call the CTX to delete the attachment
                var deleteResponse = await transactionInvoker.ExecuteAsync<CrudAttachmentContentRequest, CrudAttachmentContentResponse>(deleteRequest);
                if (deleteResponse == null)
                {
                    string error = "No response received deleting attachment";
                    logger.Error(string.Format("{0}, attachment {1} ({2})", error, attachment.Id, attachment.Name));
                    throw new RepositoryException(error);
                }
                if (!string.IsNullOrEmpty(deleteResponse.ErrorMsg))
                {
                    string error = "Error occurred deleting attachment";
                    logger.Error(string.Format("{0}, attachment {1} ({2}) : error = {3}", error, attachment.Id, attachment.Name, deleteResponse.ErrorMsg));
                    throw new RepositoryException(error);
                }
            }
            catch (ColleagueTransactionException cte)
            {
                string error = string.Format("Exception occurred deleting attachment");
                logger.Error(cte, string.Format("{0}, attachment {1} ({2}) : error = {3}", error, attachment.Id, attachment.Name, cte.Message));
                throw new RepositoryException(error);
            }
        }

        // Create attachment entities from the associated data contracts
        private IEnumerable<Attachment> BuildAttachments(IEnumerable<Attachments> attachmentRecords)
        {
            var attachmentEntities = new List<Attachment>();

            if (attachmentRecords != null && attachmentRecords.Any())
            {
                foreach (var record in attachmentRecords)
                {
                    if (record != null)
                    {
                        // create the entity
                        var attachmentEntity = new Attachment(record.Recordkey, record.AttCollectionId, record.AttName,
                            record.AttContentType, record.AttSize, record.AttOwner)
                        {
                            Status = ConvertAttachmentStatusToEntity(record.AttStatus),
                            TagOne = record.AttTagOne,
                            CreatedAt = record.AttachmentsAddtime.ToPointInTimeDateTimeOffset(record.AttachmentsAdddate, colleagueTimeZone),
                            CreatedBy = record.AttachmentsAddopr,
                            ModifiedAt = record.AttachmentsChgtime.ToPointInTimeDateTimeOffset(record.AttachmentsChgdate, colleagueTimeZone),
                            ModifiedBy = record.AttachmentsChgopr,
                            DeletedAt = record.AttachmentsDeltime.ToPointInTimeDateTimeOffset(record.AttachmentsDeldate, colleagueTimeZone),
                            DeletedBy = record.AttachmentsDelopr,
                            EncrKeyId = record.AttEncrKeyId,
                            EncrType = record.AttEncrType
                        };
                        if (!string.IsNullOrEmpty(record.AttEncrContentKey))
                        {
                            attachmentEntity.EncrContentKey = Convert.FromBase64String(record.AttEncrContentKey);
                        }
                        if (!string.IsNullOrEmpty(record.AttEncrContentKey))
                        {
                            attachmentEntity.EncrIV = Convert.FromBase64String(record.AttEncrIv);
                        }

                        // add to collection
                        attachmentEntities.Add(attachmentEntity);
                    }
                }
            }

            return attachmentEntities;
        }

        // Create attachment entities, without encryption metadata, from the associated data contracts
        private IEnumerable<Attachment> BuildAttachments(IEnumerable<AttachmentsNoEncr> attachmentRecords)
        {
            var attachmentEntities = new List<Attachment>();

            if (attachmentRecords != null && attachmentRecords.Any())
            {
                foreach (var record in attachmentRecords)
                {
                    if (record != null)
                    {
                        // create the entity
                        var attachmentEntity = new Attachment(record.Recordkey, record.AttCollectionId, record.AttName,
                            record.AttContentType, record.AttSize, record.AttOwner)
                        {
                            Status = ConvertAttachmentStatusToEntity(record.AttStatus),
                            TagOne = record.AttTagOne,
                            CreatedAt = record.AttachmentsAddtime.ToPointInTimeDateTimeOffset(record.AttachmentsAdddate, colleagueTimeZone),
                            CreatedBy = record.AttachmentsAddopr,
                            ModifiedAt = record.AttachmentsChgtime.ToPointInTimeDateTimeOffset(record.AttachmentsChgdate, colleagueTimeZone),
                            ModifiedBy = record.AttachmentsChgopr,
                            DeletedAt = record.AttachmentsDeltime.ToPointInTimeDateTimeOffset(record.AttachmentsDeldate, colleagueTimeZone),
                            DeletedBy = record.AttachmentsDelopr
                        };

                        // add to collection
                        attachmentEntities.Add(attachmentEntity);
                    }
                }
            }

            return attachmentEntities;
        }

        // Convert an attachment data contract status to entity
        private AttachmentStatus ConvertAttachmentStatusToEntity(string status)
        {
            switch (status)
            {
                case attachmentActiveStatus:
                    return AttachmentStatus.Active;
                case attachmentDeletedStatus:
                    return AttachmentStatus.Deleted;
                default:
                    return AttachmentStatus.Active;
            }                    
        }

        // Convert an attachment status entity to internal
        private string ConvertAttachmentStatusToInternal(AttachmentStatus status)
        {
            switch (status)
            {
                case AttachmentStatus.Active:
                    return attachmentActiveStatus;
                case AttachmentStatus.Deleted:
                    return attachmentDeletedStatus;
                default:
                    return attachmentActiveStatus;
            }
        }

        // Rollback attachment metadata
        private async Task RollbackAttachmentAsync(Attachment attachment)
        {
            // setup the rollback request
            var rollbackRequest = new CrudAttachmentContentRequest()
            {
                Id = attachment.Id,
                Action = "FULLDELETE"
            };

            try
            {
                // call the CTX to delete the attachment
                var rollbackResponse = await transactionInvoker.ExecuteAsync<CrudAttachmentContentRequest, CrudAttachmentContentResponse>(rollbackRequest);
                if (rollbackResponse == null)
                {
                    logger.Info(string.Format("No response received rolling back attachment, attachment {0} ({1})",
                        attachment.Id, attachment.Name));
                }
                if (!string.IsNullOrEmpty(rollbackResponse.ErrorMsg))
                {
                    logger.Info(string.Format("Error occurred rolling back attachment, attachment {0} ({1}) : error = {2}",
                        attachment.Id, attachment.Name, rollbackResponse.ErrorMsg));
                }
            }
            catch (Exception e)
            {
                logger.Info(e, string.Format("Exception occurred rolling back attachment, attachment {0} ({1}) : error = {2}", 
                    attachment.Id, attachment.Name, e.Message));
            }
        }
    }
}