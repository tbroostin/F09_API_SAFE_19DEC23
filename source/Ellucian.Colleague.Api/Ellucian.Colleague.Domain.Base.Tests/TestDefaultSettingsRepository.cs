// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestDefaultSettingsRepository
    {
        private IEnumerable<DefaultSettings> _defaultSettingsData = new List<DefaultSettings>()
        {
            #region Data Setup
                    new DefaultSettings("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "1", "Default Privacy Code")
                    {
                        EthosResources = new List<Domain.Base.Entities.DefaultSettingsResource>()
                        {
                            new Domain.Base.Entities.DefaultSettingsResource()
                            {
                                Resource = "persons",
                                PropertyName = "privacyStatus"
                            }
                        },
                        SourceTitle = "Don't Release Grades",
                        SourceValue = "G",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "CORE.VALCODES",
                        ValcodeTableName = "PRIVACY.CODES",
                        FieldName = "LDMD.DEFAULT.PRIVACY.CODE",
                        SearchType = "A",
                        SearchMinLength = 3
                    },
                    new DefaultSettings("1e941552-73bf-469e-9c6f-aa4dba4a2f52", "10", "Default Colleague Value - CRS.LEVELS")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                           new DefaultSettingsResource()
                           {    Resource = "courses",
                                PropertyName = "courseLevels.id"
                           }
                        },
                        SourceTitle = "Fourth Year",
                        SourceValue = "400",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "ST.VALCODES",
                        ValcodeTableName = "COURSE.LEVELS",
                        FieldName = "LDMD.COLL.FIELD.NAME,LDMD.COLL.DEFAULT.VALUE,CRS.LEVELS"
                    },
                    new DefaultSettings("2276f4da-4c5f-4b1b-8e95-acb2ee5edf9c","11","Default Colleague Value - CSF.TEACHING.ARRANGEMENT")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            {
                                new DefaultSettingsResource()
                                {
                                Resource = "section-instructors"
                                }
                            }
                        },
                        SourceTitle = "Instructors alternate",
                        SourceValue = "A",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "ST.VALCODES",
                        ValcodeTableName = "TEACHING.ARRANGEMENTS",
                        FieldName = "LDMD.COLL.FIELD.NAME,LDMD.COLL.DEFAULT.VALUE,CSF.TEACHING.ARRANGEMENT"
                    },
                    new DefaultSettings("95fad005-70cc-46a2-81e2-4046a805f0dd", "12", "Default Colleague Value - PAC.TYPE")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "section-instructors"
                            }
                        },
                        SourceTitle = "",
                        SourceValue = "",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "ASGMT.CONTRACT.TYPES",
                        FieldName = "LDMD.COLL.FIELD.NAME,LDMD.COLL.DEFAULT.VALUE,PAC.TYPE"
                    },
                    new DefaultSettings("508fbaf3-ad54-4e7d-85aa-94a3b9185f55", "13", "Default Colleague Value - PLPP.POSITION.ID")
                    {
                       EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "section-instructors"
                            }
                        },
                        SourceTitle = "Instrumental Music Inst",
                        SourceValue = "0000072113",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "POSITION",
                        FieldName = "LDMD.COLL.FIELD.NAME,LDMD.COLL.DEFAULT.VALUE,PLPP.POSITION.ID"
                    },
                    new DefaultSettings("6a99c050-a059-4845-854a-b6340d07b720", "14", "Default Colleague Value - PLP.LOAD.PERIOD")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "section-instructors"
                            }
                        },
                        SourceTitle = "Spring 2019",
                        SourceValue = "19SP",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "LOAD.PERIODS",
                        FieldName = "LDMD.COLL.FIELD.NAME,LDMD.COLL.DEFAULT.VALUE,PLP.LOAD.PERIOD"
                    },
                    new DefaultSettings("baf9dfe9-3b0f-47d5-86d4-8b3fefcba369", "15", "Pledge Ben/Ded Code 1")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "payroll-deduction-arrangements",
                                PropertyName = "paymentTarget.commitment.type"
                            }
                        },
                        SourceTitle = "Coll Adv Pldg Pyment",
                        SourceValue = "CAPL",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "BENDED",
                        FieldName = "LDMD.BENDED.CODE,1,1"
                    },
                    new DefaultSettings("5a821701-2bec-41db-9c09-be400be523ce", "16", "Pledge Ben/Ded Code 2")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "payroll-deduction-arrangements",
                                PropertyName = "paymentTarget.commitment.type"
                            }
                        },
                        SourceTitle = "Accounts Receivable - Balance",
                        SourceValue = "ARBL",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "BENDED",
                        FieldName = "LDMD.BENDED.CODE,1,2"
                    },
                    new DefaultSettings("678ec445-d43e-4382-b6e0-0cb17a9c52a7", "17", "Pledge Ben/Ded Code 3")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "payroll-deduction-arrangements",
                                PropertyName = "paymentTarget.commitment.type"
                            }
                        },
                        SourceTitle = "",
                        SourceValue = "",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "BENDED",
                        FieldName = "LDMD.BENDED.CODE,1,3"
                    },
                    new DefaultSettings("43c85c19-f235-414d-a384-90f957a8ba51", "18", "recurringDonation Ben/Ded Code 1")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "payroll-deduction-arrangements",
                                PropertyName = "paymentTarget.commitment.type"
                            }
                        },
                        SourceTitle = "",
                        SourceValue = "",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "BENDED",
                        FieldName = "LDMD.BENDED.CODE,2,1"
                    },
                    new DefaultSettings("d6328809-16d2-47a0-9a39-98a750c42728", "19", "recurringDonation Ben/Ded Code 2")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "payroll-deduction-arrangements",
                                PropertyName = "paymentTarget.commitment.type"
                            }
                        },
                        SourceTitle = "",
                        SourceValue = "",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "BENDED",
                        FieldName = "LDMD.BENDED.CODE,2,2"
                    },
                    new DefaultSettings("3da66741-8b1d-431f-9e77-6afeabd3854d", "2", "Tuition AR Code Default")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "student-charges",
                                PropertyName = "chargeType"
                            }
                        },
                        SourceTitle = "Tuition",
                        SourceValue = "TUI",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "AR.CODES",
                        FieldName = "LDMD.CHARGE.TYPES,LDMD.DEFAULT.AR.CODES,tuition"
                    },
                    new DefaultSettings("e91c2b61-2853-42c4-b0d4-0bdf683d7016", "20", "recurringDonation Ben/Ded Code 3")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "payroll-deduction-arrangements",
                                PropertyName = "paymentTarget.commitment.type"
                            }
                        },
                        SourceTitle = "",
                        SourceValue = "",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "BENDED",
                        FieldName = "LDMD.BENDED.CODE,2,3"
                    },
                    new DefaultSettings("44d5f1fe-7581-4043-b497-b161bbcc2df0", "21", "membershipDues Ben/Ded Code 1")
                    {
                       EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "payroll-deduction-arrangements",
                                PropertyName = "paymentTarget.commitment.type"
                            }
                        },
                        FieldHelp = "Long Description for field help.",
                        SourceTitle = "Colleague Adv Membership Dues",
                        SourceValue = "CAMB",
                        EntityName = "BENDED",
                        FieldName = "LDMD.BENDED.CODE,3,1"
                    },
                    new DefaultSettings("14be1f7c-c12c-41be-8e8c-8840ced0c3c4", "22", "membershipDues Ben/Ded Code 2")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "payroll-deduction-arrangements",
                                PropertyName = "paymentTarget.commitment.type"
                            }
                        },
                        SourceTitle = "",
                        SourceValue = "",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "BENDED",
                        FieldName = "LDMD.BENDED.CODE,3,2"
                    },
                    new DefaultSettings("8aa5d41b-086e-441f-a9d8-32336ea281af", "23", "membershipDues Ben/Ded Code 3")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "payroll-deduction-arrangements",
                                PropertyName = "paymentTarget.commitment.type"
                            }
                        },
                        SourceTitle = "",
                        SourceValue = "",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "BENDED",
                        FieldName = "LDMD.BENDED.CODE,3,3"
                    },
                    new DefaultSettings("5da53040-a932-4197-b510-10ed5fe4d6ee", "24", "Course Active Status Default")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "courses"
                            }
                        },
                        FieldHelp = "Long Description for field help.",
                        SourceTitle = "Active",
                        SourceValue = "A",
                        EntityName = "ST.VALCODES",
                        ValcodeTableName = "COURSE.STATUSES",
                        FieldName = "LDMD.COURSE.ACT.STATUS"
                    },
                    new DefaultSettings("2a05951a-93a5-40d5-93af-5604c38d53f3", "25", "Course Inactive Status Default")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "courses"
                            }
                        },
                        FieldHelp = "Long Description for field help.",
                        SourceTitle = "Inactive",
                        SourceValue = "I",
                        EntityName = "ST.VALCODES",
                        ValcodeTableName = "COURSE.STATUSES",
                        FieldName = "LDMD.COURSE.INACT.STATUS"
                    },
                    new DefaultSettings("f77bd587-42e7-4b17-8ee0-b33e464248af", "26", "Default Application Status Staff")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "admission-applications"
                            },
                            new DefaultSettingsResource()
                            {
                                 Resource = "admission-application-submissions"
                            },
                            new DefaultSettingsResource()
                            {
                                 Resource = "admission-decisions"
                            }
                        },
                        SourceTitle = "Bernard Alvano",
                        SourceValue = "0003977",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "STAFF",
                        FieldName = "LDMD.DEFAULT.APPL.STAT.STAFF"
                    },
                    new DefaultSettings("19751081-4c16-4fe9-b7c6-9506943e54f7", "27", "Default Application Status")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "admission-applications"
                            },
                            new DefaultSettingsResource()
                            {
                                 Resource = "admission-application-submissions"
                            }
                        },
                        SourceTitle = "Accepted",
                        SourceValue = "AC",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "APPLICATION.STATUSES",
                        FieldName = "LDMD.DEFAULT.APPL.STATUS"
                    },
                    new DefaultSettings("e6a1e377-18e7-4b78-b244-056557e6af29", "28", "Default Prospect Application Status Staff")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "admission-applications"
                            },
                            new DefaultSettingsResource()
                            {
                                 Resource = "admission-application-submissions"
                            }
                        },
                        SourceTitle = "Bernard Alvano",
                        SourceValue = "0003977",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "STAFF",
                        FieldName = "LDMD.PRSPCT.APPL.STAT.STAFF"
                    },
                    new DefaultSettings("2acec661-15ec-4626-99d8-5b4e477da5f5", "29", "Default Prospect Application Status")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "admission-applications"
                            },
                            new DefaultSettingsResource()
                            {
                                 Resource = "admission-application-submissions"
                            }
                        },
                        SourceTitle = "Prospect",
                        SourceValue = "PR",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "APPLICATION.STATUSES",
                        FieldName = "LDMD.PRSPCT.APPL.STATUS"
                    },
                    new DefaultSettings("d4e00748-a24b-44cc-b24a-db43b711a6a4", "3", "Fee AR Code Default")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "student-charges",
                                PropertyName = "chargeType"
                            }
                        },
                        SourceTitle = "Student Activity Fee",
                        SourceValue = "ACTFE",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "AR.CODES",
                        FieldName = "LDMD.CHARGE.TYPES,LDMD.DEFAULT.AR.CODES,fee"
                    },
                    new DefaultSettings("a45da9e0-de1b-4924-9e56-fd80d9097390", "30", "Default AR Type")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource ="student-charges",
                                PropertyName = "fundingSource"
                            },
                            new DefaultSettingsResource()
                            {
                                Resource = "student-payments",
                                PropertyName = "fundingDestination"
                            }
                        },
                        SourceTitle = "Student Receivables",
                        SourceValue = "01",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "AR.TYPES",
                        FieldName = "LDMD.DEFAULT.AR.TYPE"
                    },
                    new DefaultSettings("bd05934b-34c9-4a16-8465-f363f811e421", "31", "Distribution for Cash Receipts")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "student-payments"
                            }
                        },
                        SourceTitle = "Payment/Credit on Account",
                        SourceValue = "BANK",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "RCPT.TENDER.GL.DISTR",
                        FieldName = "LDMD.DEFAULT.DISTR"
                    },
                    new DefaultSettings("a6f2a180-52f1-4427-af23-2c9698b18f10", "32", "Sponsor AR Code")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "student-payments"
                            }
                        },
                        SourceTitle = "Pam's 111 Charges",
                        SourceValue = "111",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "AR.CODES",
                        FieldName = "LDMD.SPONSOR.AR.CODE"
                    },
                    new DefaultSettings("5247db20-2e5f-4d32-a25a-19d86799c1db", "33", "Sponsor AR Type")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "student-payments"
                            }
                        },
                        SourceTitle = "Sponsor Receivable",
                        SourceValue = "03",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "AR.TYPES",
                        FieldName = "LDMD.SPONSOR.AR.TYPE"
                    },
                    new DefaultSettings("915430ee-c537-4f4d-a82c-fd8e560062ae", "34", "Default Payment Sponsor")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "student-payments"
                            }
                        },
                        SourceTitle = "Kelly Nyman",
                        SourceValue = "0014122",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "PERSON",
                        FieldName = "LDMD.DEFAULT.SPONSOR"
                    },
                    new DefaultSettings("c83267a7-6971-410b-a1d3-49b1f9e513a6", "35", "Default Payment Method")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "student-payments"
                            }
                        },
                        SourceTitle = "Elevate Payments",
                        SourceValue = "ELEV",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "PAYMENT.METHOD",
                        FieldName = "LDMD.PAYMENT.METHOD"
                    },
                    new DefaultSettings("e4ebd941-0a17-405c-8877-398c2b18586b", "36", "Default Residence Life AR Type")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource ="housing-assignments"
                            },
                            new DefaultSettingsResource()
                            {
                                Resource = "meal-plan-assignments"
                            }
                        },
                        SourceTitle = "Student Receivables",
                        SourceValue = "01",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "AR.TYPES",
                        FieldName = "LDMD.DFLT.RES.LIFE.AR.TYPE"
                    },
                    new DefaultSettings("dec2a961-6522-421d-adc5-126a6f5ee470", "4", "Housing AR Code Default")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "student-charges",
                                PropertyName = "chargeType"
                            }
                        },
                        SourceTitle = "Residence Hall Charges",
                        SourceValue = "RESHL",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "AR.CODES",
                        FieldName = "LDMD.CHARGE.TYPES,LDMD.DEFAULT.AR.CODES,housing"
                    },
                    new DefaultSettings("16479763-141d-4bb9-b053-105433bed9f4", "5", "Meal AR Code Default")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "student-charges",
                                PropertyName = "chargeType"
                            }
                        },
                        SourceTitle = "Meal Plan Charges",
                        SourceValue = "MEALS",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "AR.CODES",
                        FieldName = "LDMD.CHARGE.TYPES,LDMD.DEFAULT.AR.CODES,meal"
                    },
                    new DefaultSettings("6d0b45c8-d37a-440a-9d96-23cffef95fca", "6", "Default Colleague Value - CRS.APPROVAL.AGENCY.IDS")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "courses"
                            }
                        },
                        SourceTitle = "Ellucian University 2nd Line Of Name",
                        SourceValue = "0000043",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "CORP.FOUNDS",
                        FieldName = "LDMD.COLL.FIELD.NAME,LDMD.COLL.DEFAULT.VALUE,CRS.APPROVAL.AGENCY.IDS"
                    },
                    new DefaultSettings("3d6b036c-1a75-4f08-a1db-e9d1ab607d0b", "7", "Default Colleague Value - CRS.APPROVAL.IDS")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "courses"
                            }
                        },
                        SourceTitle = "Steven Magnusson",
                        SourceValue = "0003582",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "STAFF",
                        FieldName = "LDMD.COLL.FIELD.NAME,LDMD.COLL.DEFAULT.VALUE,CRS.APPROVAL.IDS"
                    },
                    new DefaultSettings("1bbd659b-c3c0-4f0b-acf6-70d2113f573c", "8", "Default Colleague Value - CRS.CRED.TYPE")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "courses",
                                PropertyName = "credits.creditCategory.detail.id"
                            }
                        },
                        SourceTitle = "Continuing Education",
                        SourceValue = "CE",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "CRED.TYPES",
                        FieldName = "LDMD.COLL.FIELD.NAME,LDMD.COLL.DEFAULT.VALUE,CRS.CRED.TYPE"
                    },
                    new DefaultSettings("b307f763-22f6-41e6-bd25-8fdb434bd53d", "9", "Default Colleague Value - CRS.ACAD.LEVEL")
                    {
                        EthosResources = new List<DefaultSettingsResource>()
                        {
                            new DefaultSettingsResource()
                            {
                                Resource = "courses",
                                PropertyName = "academicLevel.id"
                            }
                        },
                        SourceTitle = "Undergraduate",
                        SourceValue = "UG",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "ACAD.LEVELS",
                        FieldName = "LDMD.COLL.FIELD.NAME,LDMD.COLL.DEFAULT.VALUE,CRS.ACAD.LEVEL"
                    }
                    #endregion
        };

        public IEnumerable<DefaultSettings> GetDefaultSettingsAsync(bool bypassCache)
        {
            return _defaultSettingsData;
        }

        public DefaultSettings GetDefaultSettingsByGuidAsync(string guid, bool bypassCahce)
        {
            var defaultSettings = _defaultSettingsData.Where(ds => ds.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            return defaultSettings;
        }

        public string GetDefaultSettingsIdFromGuidAsync(string guid)
        {
            var defaultSettings = _defaultSettingsData.Where(ds => ds.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (defaultSettings == null)
            {
                return null;
            }

            return defaultSettings.Code;
        }

        public DefaultSettings UpdateDefaultSettingsAsync(DefaultSettings defaultSettings)
        {
            var guid = defaultSettings.Guid;
            var defaultSettingsEntity = _defaultSettingsData.Where(ds => ds.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            return defaultSettingsEntity;
        }

        public Dictionary<string, string> GetAllArCodesAsync(bool bypassCache)
        {
            var dictionary = new Dictionary<string, string>()
            {
                { "TUI", "Tuition" },
                { "ACTFE", "Student Activity Fee" },
                { "111", "Pam's 111 Charges" },
                { "MEALS", "Meal Plan Charges" },
                { "RESHL", "Residence Hall Charges" }
            };
            return dictionary;
        }

        public Dictionary<string, string> GetAllArTypesAsync(bool bypassCache)
        {
            var dictionary = new Dictionary<string, string>()
            {
                { "01", "Student Receivables" },
                { "03", "Sponsor Receivable" }
            };
            return dictionary;
        }

        public Dictionary<string, string> GetAllApprovalAgenciesAsync(bool bypassCache)
        {
            var dictionary = new Dictionary<string, string>()
            {
                { "0000043", "Ellucian University 2nd Line Of Name" }
            };
            return dictionary;
        }

        public Dictionary<string, string> GetAllStaffApprovalsAsync(bool bypassCache)
        {
            var dictionary = new Dictionary<string, string>()
            {
                { "0003582", "Steven Magnusson" }
            };
            return dictionary;
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

        public Dictionary<string, string> GetAllAssignmentContractTypesAsync(bool bypassCache)
        {
            var dictionary = new Dictionary<string, string>()
            {
                { "FT", "Full Time" }
            };
            return dictionary;
        }

        public Dictionary<string, string> GetAllPositionsAsync(bool bypassCache)
        {
            var dictionary = new Dictionary<string, string>()
            {
                { "0000072113", "Instrumental Music Inst" }
            };
            return dictionary;
        }

        public Dictionary<string, string> GetAllLoadPeriodsAsync(bool bypassCache)
        {
            var dictionary = new Dictionary<string, string>()
            {
                { "19SP", "Spring 2019" }
            };
            return dictionary;
        }

        public Dictionary<string, string> GetAllBendedCodesAsync(bool bypassCache)
        {
            var dictionary = new Dictionary<string, string>()
            {
                { "CAPL", "Coll Adv Pldg Pyment" },
                { "ARBL", "Accounts Receivable - Balance" },
                { "CARP", "Coll Adv Recurring Pldg Pymnt" },
                { "CAMB", "Colleague Adv Membership Dues" }
            };
            return dictionary;
        }

        public Dictionary<string, string> GetAllApplicationStaffAsync(bool bypassCache)
        {
            var dictionary = new Dictionary<string, string>()
            {
                { "0003977", "Bernard Alvano" }
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

        public Dictionary<string, string> GetAllReceiptTenderGlDistrsAsync(bool bypassCache)
        {
            var dictionary = new Dictionary<string, string>()
            {
                { "BANK", "Payment/Credit on Account" }
            };
            return dictionary;
        }

        public Dictionary<string, string> GetAllPaymentMethodsAsync(bool bypassCache)
        {
            var dictionary = new Dictionary<string, string>()
            {
                { "ELEV", "Elevate Payments" }
            };
            return dictionary;
        }

        public Dictionary<string, string> GetAllSponsorsAsync(bool bypassCache)
        {
            var dictionary = new Dictionary<string, string>()
            {
                { "0014122", "Kelly Nyman" }
            };
            return dictionary;
        }

        public Dictionary<string, string> GetAllValcodeItemsAsync(string entity, string valcodeTable, bool bypassCache, string specialProcessing = "*")
        {
            var dictionary = new Dictionary<string, string>();
            switch (valcodeTable)
            {
                case "PRIVACY.CODES":
                    dictionary.Add("G", "Don't Release Grades");
                    break;
                case "COURSE.LEVELS":
                    dictionary.Add("400", "Fourth Year");
                    break;
                case "TEACHING.ARRANGEMENTS":
                    dictionary.Add("A", "Instructors alternate");
                    break;
                case "COURSE.STATUSES":
                    dictionary.Add("A", "Active");
                    dictionary.Add("I", "Inactive");
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