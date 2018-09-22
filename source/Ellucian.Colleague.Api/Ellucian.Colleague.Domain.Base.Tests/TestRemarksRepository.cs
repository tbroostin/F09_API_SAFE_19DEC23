// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestRemarksRepository
    {
        private string[,] remarks = {
                                            //GUID                                   AUTHOR     CODE   DONORID   INTG   PRVT  TEXT               TYPE                              DONORID
                                            {"a3a2e49b-df50-4133-9507-ecad4e04004d", "0011905", "ST", "0013395", "BSF", "1", "This is a comment", "BU"}, 
                                            {"1708d2fd-00d2-4a5a-9f70-d4664137e946", "0011905", "ST", "0013395", "BSF", "0", "This is another comment", "PE"}, 
                                            {"7bc3f7f7-c8d4-4d89-b058-43faeb80e9f6", "0011905", "FF", "0013395", "BSF", "1", "Comments", "BU"}, 
                                            {"7295732d-676e-46ad-957a-5ccb0af6feeb", "0011905", "FA", "0013395", "BSF", "1", "This comment", "PE"}, 
                                            {"d0503a86-5bdd-4935-aeb7-9e963ad51945", "0011905", "FA", "0013395", "BSF", "0", "Comment 2", "BU"}
                                            };

        public IEnumerable<Remark> GetRemarkCode()
        {
            var remarksList = new List<Remark>();

            var items = remarks.Length / 8;

            for (int x = 0; x < items; x++)
            {
               var remark = new Remark(remarks[x, 0]);
               remark.RemarksAuthor = remarks[x, 1];
               remark.RemarksCode = remarks[x, 2];
               remark.RemarksDonorId = remarks[x, 3];
               remark.RemarksIntgEnteredBy = remarks[x, 4];
               remark.RemarksPrivateType = ConvertConfidentialityCategoryToConfidentialityTypeEnum(remarks[x, 5]);
               remark.RemarksText = remarks[x, 6];
               remark.RemarksType = remarks[x, 7];

                remarksList.Add(remark);
                                         
            }
            return remarksList;
        }

        private ConfidentialityType ConvertConfidentialityCategoryToConfidentialityTypeEnum(string confidentialityCategory)
        {
            switch (confidentialityCategory)
            {
                case "0":
                    return ConfidentialityType.Public;
                case "1":
                    return ConfidentialityType.Private;
                default:
                    return ConfidentialityType.Public;
            }
        }

    }
}