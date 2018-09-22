// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestHousingRequestsRepository : IHousingRequestRepository
    {
        public List<HousingRequest> housingEntities = new List<HousingRequest>();

        private string[,] housingData = {
            //                                                                             ADMIT  INST                          APPL    START
            //GUID                                   ID   APPLICANT  PROGRAM    REP        STATUS ATTEND    COMMENT  LOCATIONS  NO      TERM      LOAD  STATUS   ADMIT SOURCE WITHDRAW RESIDENCY
            {"1c5bbcbc-80e3-8151-4042-db9893ac337a", "1", "0003748", "BA-MATH", "0003849", "FR", "0004899", "",      "DT",      "3430", "2017/SP", "F", "MS,AD", "AD", "EDX", "",   "IN"},
            {"138951cc-459e-7912-a065-0471a7a2c644", "2", "0006374", "AA-NURS", "0003849", "GD", "",        "",      "MC,DT",   "2293", "2017/FA", "P", "RE",    "",    "SV",  "AC", "IN"},
            {"fbdfac70-88a0-69a1-4362-62ea5cdafd69", "3", "0037487", "MA-LAW",  "",        "ND", "",        "",      "",        "3345", "2017/SP", "F", "AC,AP", "AD",  "WI",  "",   "CC"},
            {"d328fd10-9c90-b1a3-4a2f-543bc099be37", "4", "2003894", "MS-SCI",  "",        "TR", "",        "",      "",        "4490", "2017/FA", "F", "AC,AD", "",    "EDX", "FP", "RE"}
        };


        public void Populate()
        {
            // There are 17 fields for each application in the array
            var items = housingData.Length / 17;

            for (int x = 0; x < items; x++)
            {
                var housing = new HousingRequest(housingData[x, 0], housingData[x, 1], DateTimeOffset.Now, housingData[x, 5]);
                housing.PersonId = housingData[x, 2];
                housing.LotteryNo = Convert.ToInt64(housingData[x, 9]);
                housing.Term = housingData[x, 10];
                housing.EndDate = DateTimeOffset.Now;
                var aLocations = new List<string>();
                List<string> locations = housingData[x, 8].Split(',').ToList();
                foreach (var d in locations)
                {
                    aLocations.Add(d);
                }
                //housing.BuildingPreference = aLocations;
                housing.RoomPreferences = new List<RoomPreference>();
                List<string> statuses1 = housingData[x, 12].Split(',').ToList();
                foreach (var s in statuses1)
                {
                    housing.RoomPreferences.Add(new RoomPreference()
                    {
                        Building = s,
                        BuildingReqdFlag = "flag",
                        Floor = "2",
                        //FloorCharacteristic = "characteristics",
                        //FloorCharacteristicReqd = "charReqd",
                        FloorReqd = "floorReqd",
                        Room = "room",
                        //RoomCharacteristic = "roomChar",
                        //RoomCharacteristicReqdFlag = "roomCharReqd",
                        RoomReqdFlag = "roomReqd",
                        SiteReqdFlag = "siteReqd",
                        Wing = "wing",
                        WingReqdFlag = "wingReqd"
                    });
                }
                housing.RoommatePreferences = new List<RoommatePreference>();
                List<string> statuses2 = housingData[x, 12].Split(',').ToList();
                foreach (var s in statuses2)
                {
                    housing.RoommatePreferences.Add(new RoommatePreference()
                    {
                        RoommateId = housingData[x, 2],
                        RoommateRequired = s
                    });
                }
                housing.RoommateCharacteristicPreferences = new List<RoommateCharacteristicPreference>();
                List<string> statuses3 = housingData[x, 12].Split(',').ToList();
                foreach (var s in statuses3)
                {
                    housing.RoommateCharacteristicPreferences.Add(new RoommateCharacteristicPreference()
                    {
                        RoommateCharacteristic = housingData[x, 2],
                        RoommateCharacteristicRequired = s
                    });
                }

                housingEntities.Add(housing);
            }
        }

        public Task<HousingRequest> GetHousingRequestByGuidAsync(string guid)
        {
            Populate();
            var appl = housingEntities.FirstOrDefault(p => p.Guid == guid);
            if (appl != null)
            {
                return Task.FromResult(appl);
            }
            throw new KeyNotFoundException(string.Format("Housing Request {0} not found in person", guid));
        }

        public Task<Tuple<IEnumerable<HousingRequest>, int>> GetHousingRequestsAsync(int offset, int limit, bool bypassCache)
        {
            Populate();
            var totalRecords = housingEntities.Count();
            return Task.FromResult(new Tuple<IEnumerable<HousingRequest>, int>(housingEntities, totalRecords));
        }

        public Task<Dictionary<string, string>> GetPersonGuidsAsync(IEnumerable<string> aptitudeAssessmentKeys)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            return Task.FromResult(dictionary);
        }

        public Task<IDictionary<string, string>> GetStaffOperIdsAsync(List<string> ownerIds)
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>();
            return Task.FromResult(dictionary);
        }

        /// <summary>
        /// Dictionary of string, string that contains the Ethos Extended Data to send into the CTX
        /// key is column name
        /// value is value to save in, if empty string then this means it is meant to remove the data from colleague
        /// </summary>
        public Dictionary<string, string> EthosExtendedDataDictionary { get; set; }

        /// <summary>
        /// Takes the EthosExtendedDataList dictionary and splits it into two List string to be passed to Colleague CTX 
        /// </summary>
        /// <returns>T1 is the list of keys, T2 is a list values that match up. Returns null if the list is empty</returns>
        public Tuple<List<string>, List<string>> GetEthosExtendedDataLists()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetHousingRequestKeyAsync(string guid)
        {
            throw new NotImplementedException();
        }


        public Task<HousingRequest> UpdateHousingRequestAsync(HousingRequest housingRequestEntity)
        {
            throw new NotImplementedException();
        }
    }
}
