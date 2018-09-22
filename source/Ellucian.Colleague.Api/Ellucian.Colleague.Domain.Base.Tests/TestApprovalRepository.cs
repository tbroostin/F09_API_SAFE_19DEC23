// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestApprovalRepository : IApprovalRepository
    {
        public class approvalDocumentRecord
        {
            public string recordKey;
            public string personId;
            public string text;
        }

        public IEnumerable<approvalDocumentRecord> approvalDocuments = new List<approvalDocumentRecord>()
        {
            new approvalDocumentRecord()
            {
                recordKey = "REGDOC",
                personId = "0000001",
                text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Morbi a lectus arcu. Maecenas vulputate at neque consectetur vestibulum. Fusce commodo quam est, vitae pharetra urna semper sed. Suspendisse sagittis mauris ut placerat pretium. Morbi ac enim porttitor, convallis neque ac, dictum mauris. Proin commodo rutrum ipsum non fringilla. Quisque suscipit quis nibh at sagittis. Quisque ac lacus a neque mollis facilisis. Phasellus maximus metus purus, eu sagittis elit cursus ut. Nam sed metus quis leo facilisis tristique ac sit amet urna. Integer tincidunt libero tellus. Nulla venenatis massa a eleifend viverra."
            },
            new approvalDocumentRecord()
            {
                recordKey = "DIFFDOC",
                personId = "0000001",
                text = "Ut maximus, quam at egestas porta, magna diam placerat neque, id fringilla nulla dui et augue. Maecenas et maximus lacus. Mauris at mi a diam sagittis iaculis sed a dui. Sed pretium mauris et libero interdum, ac molestie lacus consequat. Proin porta pellentesque dui, in cursus ex tristique et. Integer ullamcorper augue elit. Integer tincidunt libero tellus. Nulla venenatis massa a eleifend viverra."
            },
            new approvalDocumentRecord()
            {
                recordKey = "AWESOMEDOC",
                personId = "0003315",
                text = "Aenean sed velit diam. Sed lacus lacus, mattis at tellus ut, efficitur varius turpis. Aenean hendrerit ullamcorper mauris at malesuada."
            }
        };

        public class approvalResponseRecord
        {
            public string recordKey;
            public string userId;
            public string personId;
            public string document;
            public DateTimeOffset date;
            public DateTime? time;
            public bool approved;
        }

        public IEnumerable<approvalResponseRecord> responseRecords = new List<approvalResponseRecord>()
        {
            new approvalResponseRecord()
            {
                recordKey = "resp1",
                userId = "hgf",
                personId = "0000001",
                document = "document",
                date = new DateTime(),
                approved = true
            },
            new approvalResponseRecord()
            {
                recordKey = "resp2",
                userId = "dfg",
                personId = "0000001",
                document = "document",
                date = new DateTime(),
                approved = false
            },
            new approvalResponseRecord()
            {
                recordKey = "resp3",
                userId = "hgf",
                personId = "0003315",
                document = "document 3",
                date = new DateTime(),
                approved = true
            }
        };
        public ApprovalDocument CreateApprovalDocument(ApprovalDocument approvalDoc)
        {
            throw new NotImplementedException();
        }

        public ApprovalResponse CreateApprovalResponse(ApprovalResponse approvalResp)
        {
            throw new NotImplementedException();
        }

        public ApprovalDocument GetApprovalDocument(string id)
        {
            var documentRecord = approvalDocuments.FirstOrDefault(d => d.recordKey == id);
            var docText = new List<string>() { documentRecord != null ? documentRecord.text : null };
            return new ApprovalDocument(id, docText)
            {
                PersonId = documentRecord.personId
            };
        }

        public ApprovalResponse GetApprovalResponse(string id)
        {
            var responseRecord = responseRecords.FirstOrDefault(r => r.recordKey == id);
            return new ApprovalResponse(id, responseRecord.document, responseRecord.personId, responseRecord.userId, responseRecord.date, responseRecord.approved);
        }
    }
}
