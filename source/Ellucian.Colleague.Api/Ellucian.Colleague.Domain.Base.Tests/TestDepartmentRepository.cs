// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestDepartmentRepository
    {
        private readonly string[,] _departments =
        {
            //GUID                   //ID         DESC														                                           
            {"40052c84-9f25-4f08-bd13-48e2a2ec4f49", "AAPMA", "DO NOT USE", "INDUS"},
            {"1a2e2906-d46b-4698-80f6-af87b8083c64", "ADMIS", "Admissions", "TRAIN"},
            {"a36dbff6-6a2e-43f4-ad80-b7c214de799b", "AGBU", "Agriculture Business", "INDUS"},
            {"dcd21389-7f9f-4b35-a701-1a50fa6f1bce", "AGME", "Agriculture Mechanics", "INDUS"},
            {"a7c2a84f-315f-4b10-94d1-07d56215159d", "ANSC", "Animal Science", "INDUS"},
            {"ba7d1b7d-ab81-4f9f-8f3b-83f4c9031f89", "AOJU", "Administration of Justice", "INDUS"},
            {"69882da8-39b5-4731-b624-de0047657986", "APMA", "AP Manager", "TRAIN"},
            {"32402fbe-9263-4080-b21a-f1eeef6df54f", "ART", "Art","ART"},
            {"8f4cf9ad-104a-4f14-94d9-b6185a7b1f57", "ARTH", "Art History", "ART"},
            {"ccc027c3-1f00-4e22-bb70-87a5ee2d90e9", "AUTO", "Automotive Technology", "INDUS"},
            {"49ac9882-8870-4863-b0e5-d74c377696f5", "AVIA", "Aviation", "INDUS"},
            {"f5cfb24c-f1ff-4fa4-8dbd-8700b397b7f2", "BIOC", "Biochemistry", "INDUS"},
            {"6d6040a5-1a98-4614-943d-ad20101ff057", "BIOL", "Biology", "INDUS"},
            {"fad1f0dc-1ea9-4435-9630-24eb57089fbb", "BOOK", "Book Store", "INDUS"},
            {"692ebca9-d1a5-466e-afdf-31a1b9636239", "BURS", "Bursar", "TRAIN"},
            {"c2297d08-cb8e-4f3e-8bcb-ea3296f205cc", "BUSN", "Business Administration", "INDUS"},
            {"f3ff0425-493f-4bad-a5e8-4cb5a5495879", "CHEM", "Chemistry", "INDUS"},
            {"733597f7-3a2d-4010-8f31-b4b0eb0de202", "CLAS", "Classical Studies", "INDUS"},
            {"0760e86e-2824-402d-9f6b-533269bef2ea", "CNED", "Continuing Education", "INDUS"},
            {"1953ffd1-22ad-4fe2-82e0-aa77b27fddc6", "COMM", "Communications", "INDUS"},
            {"11eaf81e-a3e0-4a90-8f7b-cd2280135aba", "COMP", "Computer Science", "INDUS"},
            {"07552fa1-23d1-4ee3-ba90-c5713a0da344", "CRIM", "Criminology", "INDUS"},
            {"72a1444b-2ac8-4c2e-8724-de0f08b60852", "CROP", "Crop Science", "INDUS"},
            {"e53047b1-ad01-4bee-9d57-9fa2b8b123c8", "CULS", "Culinary Studies", "INDUS"},
            {"4fb1aecb-05ae-4d54-9c31-f36eb61a4adf", "DENT", "Dental Hygiene", "TRAIN"},
            {"31bfdeb4-9ce5-4af0-a482-6cd180d86a71", "ECED", "Early Childhood Education", "EDUCA"},
            {"59b8b8d9-5c85-40ea-8bb0-f4ea8034f5e4", "ECON", "Economics", "INDUS"},
            {"b03914b8-17d8-4ff0-beb1-c39118944648", "EDUC", "Education", "EDUCA"},
            {"2b193f38-f582-4fd5-85a4-c05786701edf", "ENGL", "English", "INDUS"},
            {"549e2aed-a31a-46f0-9614-f3bb90034a07", "ENGR", "Engineering", "INDUS"},
            {"c9005a99-43b6-4b67-9a72-154c0dc4307c", "FIAF", "VP Fiscal Affairs", "INDUS"},
            {"d8b65fc5-47dc-44ab-8d6a-08dbe892bb5d", "FIDI", "Finance Director", "INDUS"},
            {"0e344c1b-1ceb-4eed-b38a-cf513d6a0d42", "FILM", "Film Studies", "INDUS"},
            {"26cab859-4fe3-4636-b404-06dc720e9497", "FORE", "Forestry", "INDUS"},
            {"715eb242-f55a-4aae-b059-449987a6cd98", "GEOL", "Geology", "INDUS"},
            {"0c4b824c-b6f9-4b0e-bad1-65b9799b35be", "HIST", "History", "TRAIN"},
            {"e4fba8ad-0274-4bf7-b74f-9a9f0e8165d7", "HLTH", "Health Care Administration", "INDUS"},
            {"19872093-82a4-4650-91ee-e31ace3a08bf", "HRPR", "Human Resources/Payroll", "INDUS"},
            {"aa81b679-09e2-4250-bfed-8db4247ec5d9", "HUMT", "Humanities", "INDUS"},
            {"786326a4-25d2-43f5-b48c-3e7469618c8b", "INST", "Inventory Store", "INDUS"},
            {"6d050a11-40dc-4c93-bd3e-961317753b17", "INTL", "International Programs", "INDUS"},
            {"9c717e7c-0e22-4408-8196-f721bd6776a0", "ITS", "Information Technology Service", "TRAIN"},
            {"98d31bf3-73fb-4147-afe0-8ed71bffbef5", "LAW", "Law", "INDUS"},
            {"bf966099-4d43-4dba-aded-9c2d8e05256d", "MATH", "Mathematics", "INDUS"},
            {"5a1e5fec-15f7-4b45-b10a-501dd6104f36", "MDLL", "Modern Language and Literature", "INDUS"},
            {"3d665bb8-0746-48db-8f6a-7d254ff94a0f", "MEDT", "Medical Lab Technology", "INDUS"},
            {"3e007822-1306-4595-9db4-d0d32756b891", "MGMT", "Management", "INDUS"},
            {"95192db2-fd51-4a38-803e-7fe9559cd357", "MKTG", "Marketing Department", "INDUS"},
            {"951586a1-52c5-4224-8997-c61c1f56c227", "MORT", "Mortuary Science", "INDUS"},
            {"3b9dd0c4-6e42-4fb7-8bda-d648511efd8c", "MUSC", "Music", "INDUS"},
            {"10cc4f95-ad3d-4cdf-82f7-441e56176a5b", "NURS", "Nursing", "INDUS"},
            {"f001a30d-1baf-4894-ad63-98984f4a6cdf", "PARA", "Paralegal Technology", "TRAIN"},
            {"8fd41f07-8fd2-4460-a9ba-617a83b511d8", "PARK", "Park Ranger Technology", "INDUS"},
            {"e08a59e4-709c-46a9-a333-a9b680a8a38e", "PERF", "Performing Arts", "INDUS"},
            {"7f057126-22c9-4fec-b37a-d37dc6db73a5", "PHED", "Physical Education", "INDUS"},
            {"e5899f96-1c36-4062-9886-32dce0d9a99a", "PHIL", "Philosophy", "INDUS"},
            {"b37384f5-2958-4cd9-9948-38b4aa81aefb", "PHPL", "Physical Plant/Maintenance", "INDUS"},
            {"9c1d632d-c972-4481-90e0-21e3493f1629", "PHYS", "Physics", "INDUS"},
            {"c99a94c6-8d0a-40fd-9cd8-e8bc83de841b", "POLI", "Political Science", "INDUS"},
            {"c28c9c36-332e-451e-8485-dccb47a9597f", "PRES", "President's Office", "INDUS"},
            {"68e5e8c1-0eea-4b56-be53-c633ef5b7db4", "PRIN", "Print Shop", "TRAIN"},
            {"3a628ee2-575f-4ce7-b23a-942308ee7686", "PSYC", "Psychology", "TRAIN"},
            {"3ea2b7c7-31f4-49dd-815d-fceed5bf816f", "PURC", "Purchasing", "TRAIN"},
            {"9eb8eb06-d611-4c70-a8e2-0b4bb913ed20", "REAL", "Real Estate/Land Appraisal", "INDUS"},
            {"ee602847-9474-4280-8dcc-9b9a2e2f25f3", "REG", "Records and Registration", "INDUS"},
            {"bcd33020-66e8-445f-b761-b42b0e5403ba", "RELG", "Religious Studies", "INDUS"},
            {"b75cdfdc-4086-4d95-ac4d-842a1c700c93", "SECU", "Security Services", "INDUS"},
            {"750bfdad-9c9d-42d2-8ff5-f30e7fd4ce45", "SOCI", "Sociology", "INDUS"},
            {"25a9266b-f028-4697-8b6b-c8f7b1ad0669", "SZI", "Sziede Department", "INDUS"},
            {"17637068-7a3d-4f6b-9332-993ae5432e5e", "TRUK", "Truck Driving Technology", "INDUS"},
            {"8af3c406-7718-4b94-954b-ec9feff0ed3c", "UNDC", "Undecided", "INDUS"},
            {"6c1129dd-f22f-4614-b9ef-2596685da663", "WELD", "Welding", "INDUS"},
        };

        public IEnumerable<Department> Get()
        {
            var depts = new List<Department>();

            // There are 4 fields for each department in the array
            var items = _departments.Length/4;

            for (var x = 0; x < items; x++)
            {
                var guid = Guid.NewGuid().ToString();
                depts.Add(new Department(_departments[x, 0], _departments[x, 1], _departments[x, 2], true) { Division = _departments[x, 3] , InstitutionId = "0000043"} );
            }
            return depts;
        }
    }
}