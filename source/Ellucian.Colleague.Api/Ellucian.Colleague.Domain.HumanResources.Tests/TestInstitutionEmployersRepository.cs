// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    //public class TestInstitutionEmployersRepository : IInstitutionEmployersRepository
    public class TestInstitutionEmployersRepository
    {

        public class InstitutionEmployerRecord
        {
            public string guid;
            public string employerId;
            public string preferredName;
            public string code;
            public List<string> addressLines;
            public string city;
            public string state;
            public string country;
            public string postalCode;
            public string phoneNumber;
        }

        public List<InstitutionEmployerRecord> institutionEmployerRecords = new List<InstitutionEmployerRecord>()
        {
            new InstitutionEmployerRecord()
            {
                guid = "81fda6ce-77aa-4283-a878-75bbea227937",
                employerId = "ZFIDI62100BUMA",
                preferredName = "Ellucian University",                
                addressLines = new List<string>() {"123 Main Street","Suite 400"},
                code = "x",
                city = "Reston",
                state = "VA",
                country = "USA",
                postalCode = "12345",
                phoneNumber = "7145552222"
            }
        };

        private string[,] institutionEmployers = { { "625c69ff-280b-4ed3-9474-662a43616a8a" } };

        public IEnumerable<InstitutionEmployers> GetInstitutionEmployersAsync()
        {
            var institutionEmployerEntities = new List<InstitutionEmployers>();
            if (institutionEmployerRecords == null)
            {
                return institutionEmployerEntities;
            }
            foreach (var institutionEmployerRecord in institutionEmployerRecords)
            {
                try
                {
                    institutionEmployerEntities.Add(BuildInstitutionEmployer(institutionEmployerRecord));
                }
                catch (Exception)
                {

                }
            }
            return institutionEmployerEntities;

        }

        public InstitutionEmployers BuildInstitutionEmployer(InstitutionEmployerRecord institutionEmployerRecords)
        {
            var institutionEmployer = new InstitutionEmployers(institutionEmployerRecords.guid, institutionEmployerRecords.employerId);
            institutionEmployer.PreferredName = institutionEmployerRecords.preferredName;
            institutionEmployer.Code = institutionEmployerRecords.code;
            institutionEmployer.AddressLines = institutionEmployerRecords.addressLines;
            institutionEmployer.City = institutionEmployerRecords.city;
            institutionEmployer.State = institutionEmployerRecords.state;
            institutionEmployer.Country = institutionEmployerRecords.country;
            institutionEmployer.PostalCode = institutionEmployerRecords.postalCode;
            institutionEmployer.PhoneNumber = institutionEmployerRecords.phoneNumber;
            return institutionEmployer;
        }
    }
}
