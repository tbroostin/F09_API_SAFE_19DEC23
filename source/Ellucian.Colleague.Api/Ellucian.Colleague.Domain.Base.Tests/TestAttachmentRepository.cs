// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestAttachmentRepository : IAttachmentRepository
    {
        private Collection<Attachment> _attachmentEntities = new Collection<Attachment>();
        public Collection<Attachment> AttachmentEntities
        {
            get
            {
                if (_attachmentEntities.Count == 0)
                    GenerateEntities();
                return _attachmentEntities;
            }
        }

        public string GetAttachTempFilePath()
        {
            return Path.Combine(HttpRuntime.AppDomainAppPath, "App_Data", "Temp");
        }

        private void GenerateEntities()
        {
            string[,] attachmentsData = GetAttachmentsData();
            int attachmentsCount = attachmentsData.Length / 8; // num fields
            for (int i = 0; i < attachmentsCount; i++)
            {
                // parse out the data into attachment entities
                int size;
                var attachment = new Attachment(attachmentsData[i, 0].Trim(), attachmentsData[i, 1].Trim(), attachmentsData[i, 3].Trim(), 
                    attachmentsData[i, 2].Trim(), int.TryParse(attachmentsData[i, 5].Trim(), out size) ? size : (int?)null,
                    attachmentsData[i, 4].Trim())
                {
                    Status = ConvertAttachmentStatusToEntity(attachmentsData[i, 6].Trim()),
                    TagOne = attachmentsData[i, 7].Trim()
                };
                _attachmentEntities.Add(attachment);
            }
        }

        private static string[,] GetAttachmentsData()
        {
            string[,] attachmentsTable =
            {
                // collection 1
                {"297c4460-5955-4be5-a6f0-c28f786c4894", "COLLECTION1", "application/pdf", "my.pdf", "0003888", "10000", "A", "tag1"},
                {"f7bbd166-aa4b-4bc6-b343-de0cace2cfa2", "COLLECTION1", "application/pdf", "my2.pdf", "0000001", "999999", "A", "tag1"},
                {"ccf84c4a-351f-481f-8646-28154d0867c8", "COLLECTION1", "application/pdf", "my3.pdf", "0000001", "845678", "A", "tag1"},
                {"c7f5ba50-3383-464a-8ebf-8e92389ce7b9", "COLLECTION1", "application/pdf", "my4.pdf", "0004111", "800", "A", "tag1"},
                {"fd8ba0eb-67b5-43b9-b719-d7a078541d9a", "COLLECTION1", "application/pdf", "my5.pdf", "0005896", "5465465", "A", "tag1"},
                {"c239b1f1-8883-4a74-bf8c-3b853a46c782", "COLLECTION1", "application/pdf", "my15.pdf", "0005896", "111111", "D", "tag1"},

                // collection 2
                {"25d87724-6906-4867-9463-e6f699e17f0e", "COLLECTION2", "application/pdf", "my6.pdf", "0004111", "6864454", "A", "tag1"},
                {"97bec6ac-3729-46bb-a3ef-952c71d857ea", "COLLECTION2", "application/pdf", "my7.pdf", "0005896", "5645646", "A", "tag1"},
                {"9bc1e856-c9c9-44c7-920c-87bfc16896ab", "COLLECTION2", "application/pdf", "my8.pdf", "0000001", "89898", "A", "tag1"},
                {"000b9366-f9d1-4c44-a2b3-2cc6a6496e52", "COLLECTION2", "application/pdf", "my9.pdf", "0002222", "649489", "A", "tag1"},
                {"3c23db9d-d58f-4e17-af4b-050afcaa4f00", "COLLECTION2", "application/pdf", "my10.pdf", "0002222", "94615", "A", "tag1"},

                // collection 3
                {"2977b1d4-6d37-46a6-bf2e-bcabdc56745d", "COLLECTION3", "application/pdf", "my11.pdf", "0009999", "8875", "A", "tag1"},
                {"bf86d6f9-ee83-48c0-978b-3a675e3df4ab", "COLLECTION3", "application/pdf", "my12.pdf", "0003333", "5645646", "A", "tag1"},
                {"74a1ad49-8f83-4b43-9df0-4993224f50e3", "COLLECTION3", "application/pdf", "my13.pdf", "0004444", "89898", "A", "tag1"},

                // collection 4
                {"2977b1d4-6d37-46a6-bf2e-bcabdc56745d", "INACTIVE_COLLECTION", "application/pdf", "my14.pdf", "0000001", "6864454", "A", "tag1"},
                {"bf86d6f9-ee83-48c0-978b-3a675e3df4ab", "INACTIVE_COLLECTION", "application/pdf", "my15.pdf", "0000001", "5645646", "A", "tag1"},
                {"74a1ad49-8f83-4b43-9df0-4993224f50e3", "INACTIVE_COLLECTION", "application/pdf", "my16.pdf", "0002222", "89898", "A", "tag1"}
            };
            return attachmentsTable;
        }

        private static AttachmentStatus ConvertAttachmentStatusToEntity(string status)
        {
            switch (status)
            {
                case "A":
                    return AttachmentStatus.Active;
                case "D":
                    return AttachmentStatus.Deleted;
                default:
                    return AttachmentStatus.Active;
            }
        }

        public Task<IEnumerable<Attachment>> GetAttachmentsAsync(string owner, string collectionId, string tagOne)
        {
            var attachments = AttachmentEntities.ToList();
            if (!string.IsNullOrEmpty(owner))
            {
                attachments = attachments.Where(a => a.Owner == owner).ToList();
            }
            if (!string.IsNullOrEmpty(collectionId))
            {
                attachments = attachments.Where(a => a.CollectionId == collectionId).ToList();
            }
            if (!string.IsNullOrEmpty(tagOne))
            {
                attachments = attachments.Where(a => a.TagOne == tagOne).ToList();
            }
            return Task.FromResult(attachments.AsEnumerable());
        }

        public Task<Attachment> GetAttachmentByIdWithEncrMetadataAsync(string attachmentId)
        {
            return Task.FromResult(AttachmentEntities.Where(a => a.Id == attachmentId).FirstOrDefault());
        }

        public Task<Attachment> GetAttachmentByIdNoEncrMetadataAsync(string attachmentId)
        {
            return Task.FromResult(AttachmentEntities.Where(a => a.Id == attachmentId).FirstOrDefault());
        }

        public Task<string> GetAttachmentContentAsync(Attachment attachment)
        {
            return Task.FromResult("C:\testpath");
        }

        public Task<Attachment> PostAttachmentAsync(Attachment attachment)
        {
            // can't use ID since it's generated
            return Task.FromResult(AttachmentEntities.Where(a => a.Name == attachment.Name).FirstOrDefault());
        }

        public Task<Attachment> PostAttachmentAndContentAsync(Attachment attachment, Stream attachmentContentStream)
        {
            // can't use ID since it's generated
            return Task.FromResult(AttachmentEntities.Where(a => a.Name == attachment.Name).FirstOrDefault());
        }

        public Task<Attachment> PutAttachmentAsync(Attachment attachment)
        {
            return Task.FromResult(AttachmentEntities.Where(a => a.Id == attachment.Id).FirstOrDefault());
        }

        public Task DeleteAttachmentAsync(Attachment attachment)
        {
            return Task.FromResult("n/a");
        }
    }
}