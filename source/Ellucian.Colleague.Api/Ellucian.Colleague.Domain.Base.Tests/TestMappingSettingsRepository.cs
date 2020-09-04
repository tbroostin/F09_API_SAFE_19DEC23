// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestMappingSettingsRepository
    {
        #region Data Setup
        private IEnumerable<MappingSettings> _mappingSettingsData = new List<MappingSettings>()
        {
            

            new Domain.Base.Entities.MappingSettings("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "1*PRI", "Email Types")
                    {
                        EthosResource = "email-types",
                        EthosPropertyName = "emailType",
                        Enumeration = "personal",
                        SourceTitle = "Primary",
                        SourceValue = "PRI"
                    },
                    new Domain.Base.Entities.MappingSettings("8a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbd", "14*AN", "Sessions")
                    {
                        EthosResource = "academic-periods",
                        EthosPropertyName = "category",
                        Enumeration = "term",
                        SourceTitle = "Fall",
                        SourceValue = "FA"
                    }            
        };

        private IEnumerable<MappingSettingsOptions> _mappingSettingsOptionsData = new List<MappingSettingsOptions>()
        {


            new Domain.Base.Entities.MappingSettingsOptions("8a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "1*PRI", "Email Types")
                    {
                        EthosResource = "email-types",
                        EthosPropertyName = "emailType",
                        Enumerations = new List<string>(){"personal","business","school","parent","family","sales","support","general","billing",
                            "legal","hr","media","matchingGifts","other" }                                                
                    },
                    new Domain.Base.Entities.MappingSettingsOptions("9a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbd", "14*AN", "Sessions")
                    {
                        EthosResource = "academic-periods",
                        EthosPropertyName = "category",
                        Enumerations = new List<string>(){"year", "term", "subterm" }
                    }
        };

        #endregion
        public IEnumerable<MappingSettings> GetMappingSettingsAsync(bool bypassCache)
        {
            return _mappingSettingsData;
        }

        public IEnumerable<MappingSettingsOptions> GetMappingSettingsOptionsAsync(bool bypassCache)
        {
            return _mappingSettingsOptionsData;
        }


        public MappingSettings GetMappingSettingsByGuidAsync(string guid, bool bypassCahce)
        {
            var mappingSettings = _mappingSettingsData.Where(ds => ds.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            return mappingSettings;
        }

        public string GetMappingSettingsIdFromGuidAsync(string guid)
        {
            var mappingSettings = _mappingSettingsData.Where(ds => ds.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (mappingSettings == null)
            {
                return null;
            }

            return mappingSettings.Code;
        }

        public MappingSettings UpdateMappingSettingsAsync(MappingSettings mappingSettings)
        {
            var guid = mappingSettings.Guid;
            var mappingSettingsEntity = _mappingSettingsData.Where(ds => ds.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            return mappingSettingsEntity;
        }


        public Dictionary<string, string> GetAllCreditTypesAsync(bool bypassCache)
        {
            var dictionary = new Dictionary<string, string>()
            {
                { "CE", "Continuing Education" }
            };
            return dictionary;
        }

        public Dictionary<string, string> GetAllAcademicLevelsAsync(bool bypassCache)
        {
            var dictionary = new Dictionary<string, string>()
            {
                { "UG", "Undergraduate" },
                { "GR", "Graduate" }
            };
            return dictionary;
        }

        public Dictionary<string, string> GetAllApplicationStatusesAsync(bool bypassCache)
        {
            var dictionary = new Dictionary<string, string>()
            {
                { "PR", "Prospect" },
                { "AC", "Accepted" }
            };
            return dictionary;
        }

        public Dictionary<string, List<string>> GetAllValcodeItemsAsync(string entity, string valcodeTable, bool bypassCache)
        {
            var dictionary = new Dictionary<string, List<string>>();
            switch (valcodeTable)
            {
                case "PERSON.EMAIL.TYPES":
                    dictionary.Add("PRI", new List<string>() { "Primary", "personal" });
                    break;
                case "PHONE.TYPES":
                    dictionary.Add("BU", new List<string>() { "Business", "business" });
                    break;
                case "ADREL.TYPES":
                    dictionary.Add("H", new List<string>() { "Home", "personal" });
                    break;
                case "MIL.STATUSES":
                    dictionary.Add("ACT", new List<string>() { "Active Duty", "activeDuty" });
                    break;
                case "SOCIAL.MEDIA.NETWORKS":
                    List<string> valcodeInfo = new List<string>() { "Facebook", "facebook" };
                    dictionary.Add("FB", valcodeInfo);
                    break;
                case "VISA.TYPES":
                    dictionary.Add("F1", new List<string>() { "Nonimmigrant students", "nonimmigrant" });
                    break;
                case "REHIRE.ELIGIBILITY.CODES":
                    dictionary.Add("RH", new List<string>() { "Rehire", "eligible" });
                    break;
                case "HR.STATUSES":
                    dictionary.Add("FT", new List<string>() { "Full Time", "fullTime" });
                    break;
                case "SECTION.STATUSES":;
                    dictionary.Add("A", new List<string>() { "Active", "open" });
                    break;
                case "CONTACT.MEASURES":
                    dictionary.Add("D", new List<string>() { "Day", "day" });
                    break;
                case "ROOM.ASSIGN.STATUSES":
                    dictionary.Add("A", new List<string>() { "Assigned", "assigned" });
                    dictionary.Add("R", new List<string>() { "Requested", "pending" });
                    break;
                case "MEAL.ASSIGN.STATUSES":
                    dictionary.Add("A", new List<string>() { "Assigned", "assigned" });
                    break;
                default:
                    dictionary.Add("U", new List<string>() { "Unknown" });
                    break;
            }

            return dictionary;
        }

        public Dictionary<string, string> EthosExtendedDataDictionary { get; set; }

        public Tuple<List<string>, List<string>> GetEthosExtendedDataLists()
        {
            return new Tuple<List<string>, List<string>>(new List<string>(), new List<string>());
        }
    }
}