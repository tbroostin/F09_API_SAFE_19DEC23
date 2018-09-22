// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Utility;
using slf4net;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Helper class for student repositories
    /// </summary>
    public class StudentRepositoryHelper : BaseApiRepository, IStudentRepositoryHelper
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cacheProvider">Cache provider interface</param>
        /// <param name="transactionFactory">Transaction factory interface</param>
        /// <param name="logger">Logging interface</param>
        /// <param name="apiSettings">API settings interface</param>
        public StudentRepositoryHelper(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {}

        /// <summary>
        /// Get the curriculum configuration from Colleague
        /// </summary>
        public CurriculumConfiguration CurriculumConfiguration
        {
            get
            {
                return GetOrAddToCache<CurriculumConfiguration>("CurriculumConfiguration",
                    () => { return BuildCurriculumConfiguration(); }, Level1CacheTimeoutValue);
            }
        }

        /// <summary>
        /// Instructional method codes
        /// </summary>
        public IEnumerable<InstructionalMethod> InstructionalMethods
        {
            get
            {
                return GetGuidCodeItem<InstrMethods, InstructionalMethod>("AllInstructionalMethods", "INSTR.METHODS",
                    (i, g) => new InstructionalMethod(g, i.Recordkey, i.InmDesc, i.InmOnline == "Y"), Level1CacheTimeoutValue);
            }
        }

        /// <summary>
        /// Section status codes
        /// </summary>
        public IEnumerable<SectionStatusCode> SectionStatusCodes
        {
            get
            {
                return GetValcode<SectionStatusCode>("ST", "SECTION.STATUSES",
                    ss => new SectionStatusCode(ss.ValInternalCodeAssocMember, ss.ValExternalRepresentationAssocMember,
                        ConvertSectionStatusActionToStatusType(ss.ValActionCode1AssocMember)), Level1CacheTimeoutValue);
            }
        }

        /// <summary>
        /// Convert a section status action code into a section status type
        /// </summary>
        /// <param name="action">The action code of the section status</param>
        /// <returns>The section status</returns>
        public SectionStatus ConvertSectionStatusActionToStatusType(string action)
        {
            if (string.IsNullOrEmpty(action))
            {
                return SectionStatus.Inactive;
            }
            switch (action)
            {
                case "1":
                    return SectionStatus.Active;
                case "2":
                    return SectionStatus.Cancelled;
                default:
                    return SectionStatus.Inactive;
            }
        }

        /// <summary>
        /// Convert a section status code into a status type
        /// </summary>
        /// <param name="code">Section status code</param>
        /// <returns>Status type</returns>
        public SectionStatus ConvertSectionStatusCodeToStatusType(string code)
        {
            var statuses = SectionStatusCodes;
            var status = statuses.FirstOrDefault(s => s.Code == code);
            
            return status == null || !status.StatusType.HasValue ? SectionStatus.Inactive : status.StatusType.Value;
        }

        /// <summary>
        /// Convert a status type into a status code
        /// </summary>
        /// <param name="status">The status type</param>
        /// <returns>The status code</returns>
        public string ConvertSectionStatusTypeToStatusCode(SectionStatus status)
        {
            var statuses = SectionStatusCodes;
            SectionStatusCode entry = null;
            switch (status)
            {
                case SectionStatus.Active:
                    entry = statuses.FirstOrDefault(x => x.StatusType == SectionStatus.Active);
                    return entry == null ? null : entry.Code;
                case SectionStatus.Cancelled:
                    entry = statuses.FirstOrDefault(x => x.StatusType == SectionStatus.Cancelled);
                    return entry == null ? null : entry.Code;
                default:
                    entry = statuses.FirstOrDefault(x => x.StatusType == SectionStatus.Inactive);
                    return entry == null ? null : entry.Code;
            }
        }

        /// <summary>
        /// Builds CDM curriculum configuration
        /// </summary>
        /// <returns>Curriculum CDM configuration</returns>
        private CurriculumConfiguration BuildCurriculumConfiguration()
        {
            LdmDefaults ldmDefaults = GetLdmDefaults();
            CdDefaults cdDefaults = GetCdDefaults();

            ExternalMapping subjectDepartmentMapping = GetExternalMapping(ldmDefaults.LdmdSubjDeptMapping);

            string approverId = null;
            string approvingAgencyId = null;
            string defaultCourseCreditTypeCode = null;
            string defaultAcademicLevelCode = null;
            string defaultCourseLevelCode = null;

            if (ldmDefaults.LdmdCollDefaultsEntityAssociation != null && ldmDefaults.LdmdCollDefaultsEntityAssociation.Count > 0)
            {
                foreach (var c in ldmDefaults.LdmdCollDefaultsEntityAssociation)
                {
                    switch (c.LdmdCollFieldNameAssocMember)
                    {
                        case "CRS.APPROVAL.IDS":
                            approverId = c.LdmdCollDefaultValueAssocMember;
                            break;
                        case "CRS.APPROVAL.AGENCY.IDS":
                            approvingAgencyId = c.LdmdCollDefaultValueAssocMember;
                            break;
                        case "CRS.CRED.TYPE":
                            defaultCourseCreditTypeCode = c.LdmdCollDefaultValueAssocMember;
                            break;
                        case "CRS.ACAD.LEVEL":
                            defaultAcademicLevelCode = c.LdmdCollDefaultValueAssocMember;
                            break;
                        case "CRS.LEVELS":
                            defaultCourseLevelCode = c.LdmdCollDefaultValueAssocMember;
                            break;
                        default:
                            break;
                    }
                }
            }

            return new CurriculumConfiguration(subjectDepartmentMapping, ldmDefaults.LdmdCourseActStatus, ldmDefaults.LdmdCourseInactStatus,
                ldmDefaults.LdmdSectionActStatus, ldmDefaults.LdmdSectionInactStatus, approvingAgencyId, approverId, defaultCourseCreditTypeCode,
                cdDefaults.CdInstrMethods, defaultAcademicLevelCode, defaultCourseLevelCode)
            {
                CourseDelimiter = cdDefaults.CdCourseDelimiter,
                WaitlistRatingCode = cdDefaults.CdWaitlistRating,
                AllowAudit = cdDefaults.CdAllowAuditFlag == "Y",
                AllowPassNoPass = cdDefaults.CdAllowPassNopassFlag == "Y",
                AllowWaitlist = cdDefaults.CdAllowWaitlistFlag == "Y",
                OnlyPassNoPass = cdDefaults.CdOnlyPassNopassFlag == "Y",
                IsInstructorConsentRequired = cdDefaults.CdFacultyConsentFlag == "Y"
            };
        }

        /// <summary>
        /// Get an external mapping
        /// </summary>
        /// <param name="id">External Mapping ID</param>
        /// <returns>The external mapping info</returns>
        private ExternalMapping GetExternalMapping(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "External Mapping ID must be specified.");
            }

            ElfTranslateTables elfTranslateTable = DataReader.ReadRecord<ElfTranslateTables>(id);
            if (elfTranslateTable == null)
            {
                throw new ArgumentOutOfRangeException("External Mapping ID " + id + " is not valid.");
            }

            return BuildExternalMapping(elfTranslateTable);
        }

        /// <summary>
        /// Build external mapping entity from a Colleague ELF translate table
        /// </summary>
        /// <param name="inData">Colleague ELF.TRANSLATE.TABLES record</param>
        /// <returns>An ExternalMapping entity</returns>
        private ExternalMapping BuildExternalMapping(ElfTranslateTables table)
        {
            if (table != null)
            {
                try
                {
                    ExternalMapping externalMapping = new ExternalMapping(table.Recordkey, table.ElftDesc)
                    {
                        OriginalCodeValidationField = table.ElftOrigCodeField,
                        NewCodeValidationField = table.ElftNewCodeField
                    };

                    if (table.ElftblEntityAssociation != null && table.ElftblEntityAssociation.Count > 0)
                    {
                        foreach (var item in table.ElftblEntityAssociation)
                        {
                            externalMapping.AddItem(new ExternalMappingItem(item.ElftOrigCodesAssocMember)
                            {
                                NewCode = item.ElftNewCodesAssocMember,
                                ActionCode1 = item.ElftActionCodes1AssocMember,
                                ActionCode2 = item.ElftActionCodes2AssocMember,
                                ActionCode3 = item.ElftActionCodes3AssocMember,
                                ActionCode4 = item.ElftActionCodes4AssocMember
                            });
                        }
                    }

                    return externalMapping;
                }
                catch (Exception ex)
                {
                    var inError = "External mapping corrupt";
                    var inString = "External mapping: " + table.Recordkey;
                    var formattedExtMap = ObjectFormatter.FormatAsXml(inString);
                    var errorMessage = string.Format("{0}" + Environment.NewLine + "{1}", inError, formattedExtMap);

                    logger.Error(ex, ex.Message);
                    logger.Info(errorMessage);
                }
            }
            return null;
        }

        private LdmDefaults GetLdmDefaults()
        {
            LdmDefaults ldmDefaults = DataReader.ReadRecord<LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS");

            if (ldmDefaults == null)
            {
                // LDM.DEFAULTS must exist for CDM configuration to function properly
                throw new ConfigurationException("CDM configuration setup not complete.");
            }
            return ldmDefaults;
        }

        private CdDefaults GetCdDefaults()
        {
            CdDefaults cdDefaults = DataReader.ReadRecord<CdDefaults>("ST.PARMS", "CD.DEFAULTS");

            if (cdDefaults == null)
            {
                // CD.DEFAULTS must exist for CDM configuration to function properly
                throw new ConfigurationException("Course Default configuration setup not complete.");
            }
            return cdDefaults;
        }
    }
}
