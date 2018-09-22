// Copyright 2014-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests
{
    public class TestIpedsInstitutionRepository : IIpedsInstitutionRepository
    {
        public class IpedsDbDataItem
        {
            public string id;
            public string unitId;
            public string name;
            public string opeId;
            public DateTime? lastModifiedDate;
        }

        public List<IpedsDbDataItem> ipedsDbDataList = new List<IpedsDbDataItem>()
        {
            new IpedsDbDataItem()
            {
                id = "1",
                unitId = "8392983",
                name = "Alabama A & M",
                opeId = "00393494",
                lastModifiedDate = new DateTime(2013, 10, 17)
            },
            new IpedsDbDataItem()
            {
                id = "2",
                unitId = "8763562",
                name = "Vermont Technical Institute",
                opeId = "00738373",
                lastModifiedDate = new DateTime(2013, 10, 18)
            },
            new IpedsDbDataItem()
            {
                id = "3",
                unitId = "9873679",
                name = "Univeral University",
                opeId = "00938938",
                lastModifiedDate = new DateTime(2013, 10, 16)
            }
        };
        
        public Task<IEnumerable<IpedsInstitution>> GetIpedsInstitutionsAsync(IEnumerable<string> opeIdList)
        {
            var distinctOpeIds = opeIdList.Distinct().Select(o => o.TrimStart('0'));

            var ipedsInstitutionEntityList = new List<IpedsInstitution>();

            foreach (var ipedsDbDataItem in ipedsDbDataList)
            {
                var lastModified = (ipedsDbDataItem.lastModifiedDate.HasValue) ? ipedsDbDataItem.lastModifiedDate.Value : DateTime.Now;
                ipedsInstitutionEntityList.Add(
                    new IpedsInstitution(ipedsDbDataItem.id, ipedsDbDataItem.unitId, ipedsDbDataItem.name, ipedsDbDataItem.opeId.TrimStart('0'), lastModified));
            }

            return Task.FromResult(ipedsInstitutionEntityList.AsEnumerable());
        }
    }
}
