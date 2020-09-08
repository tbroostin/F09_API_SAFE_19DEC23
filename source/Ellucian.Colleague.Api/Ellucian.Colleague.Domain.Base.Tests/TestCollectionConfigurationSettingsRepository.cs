// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestCollectionConfigurationSettingsRepository
    {
        private IEnumerable<CollectionConfigurationSettings> _collectionConfigurationSettingsData = new List<CollectionConfigurationSettings>()
        {
            #region Data Setup
                    new CollectionConfigurationSettings("b51d5c02-1010-44de-9fae-671ca7769170", "1", "Include in Enrolled Headcount")
                    {
                        EthosResources = new List<Base.Entities.DefaultSettingsResource>()
                        {
                            new Domain.Base.Entities.DefaultSettingsResource()
                            {
                                Resource = "section-registration-statuses",
                                PropertyName = "headcountStatus"
                            }
                        },
                        Source = new List<CollectionConfigurationSettingsSource>()
                        {
                            new CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "New",
                                SourceValue = "N"
                            },
                            new CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "Add",
                                SourceValue = "A"
                            },
                            new CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "Dropped",
                                SourceValue = "D"
                            },
                            new CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "Withdrawn",
                                SourceValue = "W"
                            }
                        },
                        FieldHelp = "For the section-registration-statuses resource, Colleague assigns...",
                        EntityName = "ST.VALCODES",
                        ValcodeTableName = "STUDENT.ACAD.CRED.STATUSES",
                        FieldName = "LDMD.INCLUDE.ENRL.HEADCOUNTS"
                    },
                    new CollectionConfigurationSettings("8f724aa8-5e7d-41dd-99b6-07d5ae4374c9","2","Office Codes")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "admission-application-supporting-item-types"
                            },
                            new DefaultSettingsResource()
                            {
                                Resource = "admission-application-supporting-types"
                            }
                        },
                        Source = new List<CollectionConfigurationSettingsSource>()
                        {
                            new CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "Admissions",
                                SourceValue = "ADM"
                            },
                            new CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "Admissions 2",
                                SourceValue = "AM"
                            }
                        },
                        FieldHelp = "Office codes in Colleague identify which communication codes and correspondence...",
                        EntityName = "CORE.VALCODES",
                        ValcodeTableName = "OFFICE.CODES",
                        FieldName = "LDMD.DFLT.ADM.OFFICE.CODES"
                    },
                    new CollectionConfigurationSettings("c84e4997-235f-4fb8-af86-ac800768f39f", "3", "Employee Benefits to Exclude")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "employees",
                                PropertyName = "benefitsStatus"
                            }
                        },
                        Source = new List<CollectionConfigurationSettingsSource>()
                        {
                            new CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "Janus Funds 403(b)",
                                SourceValue = "4JAN"
                            },
                            new CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "Standard Funds 403(b)",
                                SourceValue = "STAN"
                            }
                        },
                        FieldHelp = "You can specify one or more benefit or deduction codes to exclude...",
                        EntityName = "BENDED",
                        FieldName = "LDMD.EXCLUDE.BENEFITS"
                    },
                    new CollectionConfigurationSettings("fb8bb3c4-71ca-4f94-9054-fbf2694a4a58", "4", "HR Status Codes Denoting Leave")
                    {
                       EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "employees",
                                PropertyName = "leave"
                            }
                        },
                        Source = new List<CollectionConfigurationSettingsSource>()
                        {
                            new CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "Probation",
                                SourceValue = "PR"
                            },
                            new CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "Temporary",
                                SourceValue = "TE"
                            },
                            new CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "Student Worker",
                                SourceValue = "SW"
                            }
                        },
                        FieldHelp = "You can specify one or more human resource status codes that indicate an employee...",
                        EntityName = "HR.VALCODES",
                        ValcodeTableName = "HR.STATUSES",
                        FieldName = "LDMD.LEAVE.STATUS.CODES"
                    },
                    new CollectionConfigurationSettings("2333a672-6c2b-4dfd-9261-1e65983f75a8", "5", "Guardian Relation Types")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "person-guardians"
                            }
                        },
                        Source = new List<CollectionConfigurationSettingsSource>()
                        {
                            new CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "Guardian",
                                SourceValue = "GU"
                            },
                            new CollectionConfigurationSettingsSource()
                            {
                                SourceTitle = "Parent",
                                SourceValue = "P"
                            }
                        },
                        FieldHelp = "You can specify one or more relation types that are considered guardians...",
                        EntityName = "RELATION.TYPES",
                        FieldName = "LDMD.GUARDIAN.REL.TYPES"
                    },
                    #endregion
        };

        public IEnumerable<CollectionConfigurationSettings> GetCollectionConfigurationSettingsAsync(bool bypassCache)
        {
            return _collectionConfigurationSettingsData;
        }

        public CollectionConfigurationSettings GetCollectionConfigurationSettingsByGuidAsync(string guid, bool bypassCahce)
        {
            var collectionConfigurationSettings = _collectionConfigurationSettingsData.Where(ds => ds.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            return collectionConfigurationSettings;
        }

        public string GetCollectionConfigurationSettingsIdFromGuidAsync(string guid)
        {
            var collectionConfigurationSettings = _collectionConfigurationSettingsData.Where(ds => ds.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (collectionConfigurationSettings == null)
            {
                return null;
            }

            return collectionConfigurationSettings.Code;
        }

        public CollectionConfigurationSettings UpdateCollectionConfigurationSettingsAsync(CollectionConfigurationSettings collectionConfigurationSettings)
        {
            var guid = collectionConfigurationSettings.Guid;
            var collectionConfigurationSettingsEntity = _collectionConfigurationSettingsData.Where(ds => ds.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            return collectionConfigurationSettingsEntity;
        }

        public Dictionary<string, string> GetAllRelationTypesCodesAsync(bool bypassCache)
        {
            var dictionary = new Dictionary<string, string>()
            {
                { "A", "Nibling" },
                { "C", "Child" },
                { "CO", "Companion" },
                { "CSB", "Chld/Prnt" },
                { "CT", "Cousin" },
                { "CZ", "Contact" },
                { "DP", "Deceased Parent" },
                { "E", "Executor" },
                { "F", "Friend" },
                { "GC", "Grandchild" },
                { "GP", "Grandparent" },
                { "GU", "Guardian" },
                { "P", "Parent" },
                { "S", "Spouse" },
                { "SB", "Sibling" }
            };
            return dictionary;
        }

        public Dictionary<string, string> GetAllBendedCodesAsync(bool bypassCache)
        {
            var dictionary = new Dictionary<string, string>()
            {
                { "125D", "125 Pre-tax Dependent Spending" },
                { "125M", "125 Pre-Tax Medical Spending" },
                { "401K", "Retirement 401K Plan" },
                { "401T", "401K Ditest" },
                { "401Z", "Employer Only" },
                { "403B", "403B - With Match" },
                { "403Z", "Employee Only" },
                { "4FID", "Fedelity Funds 403(b)" },
                { "4JAN", "Janus Funds 403(b)" },
                { "STAN", "Standard Funds 403(b)" },
                { "4PUT", "Putnam Funds 403(b)" },
                { "BEN1", "Medical Benefit" },
                { "BOND", "U.S. Savings Bond" },
                { "DEEM", "Dental - Employee Only" },
                { "DEFA", "Dental - Family" },
                { "DEP1", "Dental Employee Plus One" }
            };
            return dictionary;
        }

        public Dictionary<string, string> GetAllValcodeItemsAsync(string entity, string valcodeTable, bool bypassCache)
        {
            var dictionary = new Dictionary<string, string>();
            switch (valcodeTable)
            {
                case "STUDENT.ACAD.CRED.STATUSES":
                    dictionary.Add("N", "New");
                    dictionary.Add("A", "Add");
                    dictionary.Add("D", "Dropped");
                    dictionary.Add("W", "Withdrawn");
                    dictionary.Add("X", "Deleted");
                    dictionary.Add("C", "Cancelled");
                    dictionary.Add("PR", "Preliminary Equiv Eval");
                    dictionary.Add("TR", "Transfer Equiv Eval");
                    dictionary.Add("NC", "Noncourse Equivalency");
                    dictionary.Add("DR", "Deregistered");
                    break;
                case "OFFICE.CODES":
                    dictionary.Add("AD", "Alum Devs");
                    dictionary.Add("ADM", "Admissions");
                    dictionary.Add("BUS", "Business");
                    dictionary.Add("PUR", "Purchasing");
                    dictionary.Add("REG", "Registrar");
                    dictionary.Add("PER", "Personnel");
                    dictionary.Add("PP", "Physical Plant");
                    dictionary.Add("SEC", "Campus Security");
                    dictionary.Add("INF", "Student Infirmary");
                    dictionary.Add("FA", "Financial Aid");
                    dictionary.Add("CL", "Campus Life");
                    dictionary.Add("RG", "Registrations");
                    dictionary.Add("AM", "Admissions 2");
                    break;
                case "HR.STATUSES":
                    dictionary.Add("FT", "Full-Time");
                    dictionary.Add("PT", "Part-Time");
                    dictionary.Add("RT", "Retired");
                    dictionary.Add("PR", "Probation");
                    dictionary.Add("VO", "Volunteer");
                    dictionary.Add("VF", "Visiting Faculty");
                    dictionary.Add("AF", "Adjunct Faculty");
                    dictionary.Add("CO", "COBRA Benefits");
                    dictionary.Add("SW", "Student Worker");
                    dictionary.Add("WR", "Work Retiree");
                    dictionary.Add("TE", "Temporary");
                    break;
                default:
                    dictionary.Add("U", "Unknown");
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