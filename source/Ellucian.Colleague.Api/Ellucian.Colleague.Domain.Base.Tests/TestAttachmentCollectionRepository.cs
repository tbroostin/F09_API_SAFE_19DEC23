// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestAttachmentCollectionRepository : IAttachmentCollectionRepository
    {
        private Collection<AttachmentCollection> _attachmentCollectionEntities = new Collection<AttachmentCollection>();
        public Collection<AttachmentCollection> AttachmentCollectionEntities
        {
            get
            {
                if (_attachmentCollectionEntities.Count == 0)
                {
                    GenerateEntities();
                }
                return _attachmentCollectionEntities;
            }
        }

        private void GenerateEntities()
        {
            string[,] attachmentCollectionsData = GetAttachmentCollectionsData();
            int attachmentsCount = attachmentCollectionsData.Length / 9; // num fields
            for (int i = 0; i < attachmentsCount; i++)
            {
                // parse out the data into attachment collection entities
                int maxSize;
                var attachmentCollection = new AttachmentCollection(attachmentCollectionsData[i, 0].Trim(), attachmentCollectionsData[i, 1].Trim(),
                    attachmentCollectionsData[i, 2].Trim())
                {
                    MaxAttachmentSize = int.TryParse(attachmentCollectionsData[i, 3].Trim(), out maxSize) ? maxSize : (int?)null,
                    Status = ConvertCollectionStatusToEntity(attachmentCollectionsData[i, 4].Trim()),
                    Description = attachmentCollectionsData[i, 5].Trim(),
                    AllowedContentTypes = new List<string>() { attachmentCollectionsData[i, 6].Trim() },
                    AttachmentOwnerActions = new List<AttachmentOwnerAction>() { AttachmentOwnerAction.Delete, AttachmentOwnerAction.Update },
                    Roles = new List<AttachmentCollectionIdentity>()
                    {
                        new AttachmentCollectionIdentity(attachmentCollectionsData[i, 7].Trim(), AttachmentCollectionIdentityType.Role, new List<AttachmentAction>()
                        { AttachmentAction.Create, AttachmentAction.Delete, AttachmentAction.Update, AttachmentAction.View })
                    },
                    Users = new List<AttachmentCollectionIdentity>()
                    {
                        new AttachmentCollectionIdentity(attachmentCollectionsData[i, 8].Trim(), AttachmentCollectionIdentityType.User, new List<AttachmentAction>()
                        { AttachmentAction.Create, AttachmentAction.Delete, AttachmentAction.Update, AttachmentAction.View })
                    }
                };
                _attachmentCollectionEntities.Add(attachmentCollection);
            }
        }

        private static string[,] GetAttachmentCollectionsData()
        {
            string[,] attachmentCollectionsTable =
            {
                // ID, name, owner, max size, status, description, allowed content types, role permissions, user permissions
                {"COLLECTION1", "my collection 1", "0003888", "10000", "Active", "description", "application/pdf", "31", "0000001"},
                {"COLLECTION2", "my collection 2", "0003899", "999999", "Active", "description", "application/pdf", "15", "0000001"},
                {"COLLECTION3", "my collection 3", "0003899", "999999", "Active", "description", "application/pdf", "31", "0000099"},
                {"COLLECTION4", "0000001 owner", "0000001", "999999", "Active", "description", "application/pdf", "19", "0000001"},
                {"INACTIVE_COLLECTION", "Inactive Collection", "0003899", "999999", "Inactive", "description", "application/pdf", "15", "0000001"}
            };
            return attachmentCollectionsTable;
        }

        // Convert an collection data contract status to entity
        private static AttachmentCollectionStatus ConvertCollectionStatusToEntity(string status)
        {
            switch (status)
            {
                case "Active":
                    return AttachmentCollectionStatus.Active;
                case "Inactive":
                    return AttachmentCollectionStatus.Inactive;
                default:
                    return AttachmentCollectionStatus.Active;
            }
        }

        public Task<AttachmentCollection> GetAttachmentCollectionByIdAsync(string attachmentCollectionId)
        {
            return Task.FromResult(AttachmentCollectionEntities.Where(a => a.Id == attachmentCollectionId).FirstOrDefault());
        }

        public Task<IEnumerable<AttachmentCollection>> GetAttachmentCollectionsByIdAsync(IEnumerable<string> attachmentCollectionIds)
        {
            return Task.FromResult(AttachmentCollectionEntities.Where(a => attachmentCollectionIds.Contains(a.Id)).AsEnumerable());
        }

        public Task<IEnumerable<AttachmentCollection>> GetAttachmentCollectionsByUserAsync(string personId, IEnumerable<string> roles)
        {
            return Task.FromResult(AttachmentCollectionEntities.Where(a => ( a.Owner == personId 
                || a.Users.ToList().Where(u => u.Id == personId).Any() || a.Roles.ToList().Where(r => roles.Contains(r.Id)).Any()) 
                && a.Status == AttachmentCollectionStatus.Active));
        }

        public Task<AttachmentCollection> PostAttachmentCollectionAsync(AttachmentCollection attachmentCollection)
        {
            return Task.FromResult(attachmentCollection);
        }

        public Task<AttachmentCollection> PutAttachmentCollectionAsync(AttachmentCollection attachmentCollection)
        {
            return Task.FromResult(attachmentCollection);
        }
    }
}