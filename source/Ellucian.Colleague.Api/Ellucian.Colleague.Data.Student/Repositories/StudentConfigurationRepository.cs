// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentConfigurationRepository : BaseColleagueRepository, IStudentConfigurationRepository
    {
        IEnumerable<GradingTerm> _allowedGradingTerms = null;

        public StudentConfigurationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        public async Task<StudentConfiguration> GetStudentConfigurationAsync()
        {
            StudentConfiguration studentConfiguration = await GetOrAddToCacheAsync<StudentConfiguration>("StudentConfiguration",
                async () =>
                {
                    var studentConfig = new StudentConfiguration();
                    Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS");
                    if (stwebDefaults == null)
                    {
                        var errorMessage = "Unable to access student web defaults from ST.PARMS. STWEB.DEFAULTS.";
                        logger.Info(errorMessage);
                        throw new Exception(errorMessage);
                    }
                    studentConfig.FacultyEmailTypeCode = stwebDefaults.StwebProfileFacEmailType;
                    studentConfig.FacultyPhoneTypeCode = stwebDefaults.StwebProfileFacPhoneType;
                    studentConfig.EnforceTranscriptRestriction = stwebDefaults.StwebTranVerifyRestr.Equals("Y", StringComparison.OrdinalIgnoreCase) ? true : false;
                    return studentConfig;
                });
            return studentConfiguration;
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Get the course configuration for the ELLUCIAN CDM.
        /// </summary>
        /// <returns>Course CDM configuration</returns>
        public async Task<CurriculumConfiguration> GetCurriculumConfigurationAsync()
        {
            return await GetOrAddToCacheAsync<CurriculumConfiguration>("CurriculumConfiguration",
                async () => { return await BuildCurriculumConfigurationAsync(); });
        }

        /// <summary>
        /// Get the graduation configuration information needed for a new graduation application
        /// </summary>
        /// <returns>The GraduationConfiguration entity</returns>
        public async Task<GraduationConfiguration> GetGraduationConfigurationAsync()
        {
            GraduationConfiguration studentConfiguration = await GetOrAddToCacheAsync<GraduationConfiguration>("GraduationConfiguration",
               async () =>
               {
                   var stwebDefaultsTask = DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS");
                   var graduationQuestionsTask = DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.GraduationQuestions>("GRADUATION.QUESTIONS", "");
                   var defaultsTask = DataReader.ReadRecordAsync<Data.Base.DataContracts.Defaults>("CORE.PARMS", "DEFAULTS");

                   await Task.WhenAll(stwebDefaultsTask, graduationQuestionsTask, defaultsTask);
                   Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = stwebDefaultsTask.Result;
                   Collection<Ellucian.Colleague.Data.Student.DataContracts.GraduationQuestions> graduationQuestions = graduationQuestionsTask.Result;
                   Ellucian.Colleague.Data.Base.DataContracts.Defaults defaultData = defaultsTask.Result;
                   if (stwebDefaults == null)
                   {
                       var errorMessage = "Unable to access student web defaults from ST.PARMS. STWEB.DEFAULTS.";
                       logger.Info(errorMessage);
                       throw new KeyNotFoundException(errorMessage);
                   }
                   if (defaultData == null)
                   {
                       var errorMessage = "Unable to access DEFAULTS from CORE.PARMS table.";
                       logger.Info(errorMessage);
                       throw new Exception(errorMessage);
                   }
                   return BuildGraduationConfiguration(stwebDefaults, graduationQuestions, defaultData.DefaultWebEmailType);
               });

            return studentConfiguration;
        }

        /// <summary>
        /// Get the student request configuration information needed for a new transcript request or enrollment request  
        /// </summary>
        /// <returns>The StudentRequestConfiguration entity</returns>
        public async Task<StudentRequestConfiguration> GetStudentRequestConfigurationAsync()
        {
            StudentRequestConfiguration studentRequestConfiguration = await GetOrAddToCacheAsync<StudentRequestConfiguration>("StudentRequestConfiguration",
               async () =>
               {
                   Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS");
                   Ellucian.Colleague.Data.Base.DataContracts.Defaults defaultData = await DataReader.ReadRecordAsync<Data.Base.DataContracts.Defaults>("CORE.PARMS", "DEFAULTS");

                   if (stwebDefaults == null)
                   {
                       var errorMessage = "Unable to access student web defaults from ST.PARMS. STWEB.DEFAULTS.";
                       logger.Info(errorMessage);
                       stwebDefaults = new StwebDefaults();
                   }
                   if (defaultData == null)
                   {
                       var errorMessage = "Unable to access DEFAULTS from CORE.PARMS table.";
                       logger.Info(errorMessage);
                       defaultData = new Defaults();
                   }
                   return BuildStudentRequestConfiguration(stwebDefaults, defaultData.DefaultWebEmailType);
               });

            return studentRequestConfiguration;
        }

        /// <summary>
        /// Get the faculty grading configuration information 
        /// </summary>
        /// <returns>The FacultyGradingConfiguration entity</returns>
        public async Task<FacultyGradingConfiguration> GetFacultyGradingConfigurationAsync()
        {
            FacultyGradingConfiguration configuration = await GetOrAddToCacheAsync<FacultyGradingConfiguration>("FacultyGradingConfiguration",
               async () =>
               {
                   Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS");

                   if (stwebDefaults == null)
                   {
                       var errorMessage = "Unable to access student web defaults from ST.PARMS. STWEB.DEFAULTS.";
                       logger.Info(errorMessage);
                       stwebDefaults = new StwebDefaults();
                   }

                   return await BuildFacultyGradingConfigurationAsync(stwebDefaults);
               });

            return configuration;
        }

        /// <summary>
        /// Get the course catalog configuration information needed for course catalog searches 
        /// </summary>
        /// <returns>The CourseCatalogConfiguration entity</returns>
        public async Task<CourseCatalogConfiguration> GetCourseCatalogConfigurationAsync()
        {
            CourseCatalogConfiguration configuration = await GetOrAddToCacheAsync<CourseCatalogConfiguration>("CourseCatalogConfiguration",
               async () =>
               {
                   Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS");
                   Ellucian.Colleague.Data.Student.DataContracts.CatalogSearchDefaults catalogSearchDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.CatalogSearchDefaults>("CORE.PARMS", "CATALOG.SEARCH.DEFAULTS");
                   CourseCatalogConfiguration result;
                   if (stwebDefaults == null)
                   {
                       var errorMessage = "Unable to access student web defaults from ST.PARMS. STWEB.DEFAULTS.";
                       logger.Info(errorMessage);
                       result = new CourseCatalogConfiguration(null, null);
                   }
                   else
                   {
                       // Only hide course section fee information if flag is set to No
                       bool showCatSecOtherFees = string.IsNullOrEmpty(stwebDefaults.StwebShowCatSecOtherFee) || stwebDefaults.StwebShowCatSecOtherFee.ToUpper() != "N";
                       result = new CourseCatalogConfiguration(stwebDefaults.StwebRegStartDate, stwebDefaults.StwebRegEndDate)
                       {
                           ShowCourseSectionFeeInformation = showCatSecOtherFees
                       };
                   }

                   if (catalogSearchDefaults == null)
                   {
                       var errorMessage = "Unable to access catalog search defaults from CORE.PARMS: CATALOG.SEARCH.DEFAULTS.";
                       logger.Info(errorMessage);
                   }
                   else
                   {
                       if (catalogSearchDefaults.ClsdSearchElementsEntityAssociation != null && catalogSearchDefaults.ClsdSearchElementsEntityAssociation.Any())
                       {
                           foreach (var filterOption in catalogSearchDefaults.ClsdSearchElementsEntityAssociation)
                           {
                               if (filterOption.ClsdSearchElementAssocMember != null)
                               {
                                   switch (filterOption.ClsdSearchElementAssocMember.ToUpper())
                                   {
                                       case "AVAILABILITY":
                                           result.AddCatalogFilterOption(CatalogFilterType.Availability, filterOption.ClsdHideAssocMember.ToUpper() == "Y");
                                           break;
                                       case "LOCATIONS":
                                           result.AddCatalogFilterOption(CatalogFilterType.Locations, filterOption.ClsdHideAssocMember.ToUpper() == "Y");
                                           break;
                                       case "TERMS":
                                           result.AddCatalogFilterOption(CatalogFilterType.Terms, filterOption.ClsdHideAssocMember.ToUpper() == "Y");
                                           break;
                                       case "DAYS_OF_WEEK":
                                           result.AddCatalogFilterOption(CatalogFilterType.DaysOfWeek, filterOption.ClsdHideAssocMember.ToUpper() == "Y");
                                           break;
                                       case "TIMES_OF_DAY":
                                           result.AddCatalogFilterOption(CatalogFilterType.TimesOfDay, filterOption.ClsdHideAssocMember.ToUpper() == "Y");
                                           break;
                                       case "INSTRUCTORS":
                                           result.AddCatalogFilterOption(CatalogFilterType.Instructors, filterOption.ClsdHideAssocMember.ToUpper() == "Y");
                                           break;
                                       case "ACADEMIC_LEVELS":
                                           result.AddCatalogFilterOption(CatalogFilterType.AcademicLevels, filterOption.ClsdHideAssocMember.ToUpper() == "Y");
                                           break;
                                       case "COURSE_LEVELS":
                                           result.AddCatalogFilterOption(CatalogFilterType.CourseLevels, filterOption.ClsdHideAssocMember.ToUpper() == "Y");
                                           break;
                                       case "COURSE_TYPES":
                                           result.AddCatalogFilterOption(CatalogFilterType.CourseTypes, filterOption.ClsdHideAssocMember.ToUpper() == "Y");
                                           break;
                                       case "TOPICS":
                                           result.AddCatalogFilterOption(CatalogFilterType.TopicCodes, filterOption.ClsdHideAssocMember.ToUpper() == "Y");
                                           break;
                                       case "INSTRUCTION_TYPES":
                                           result.AddCatalogFilterOption(CatalogFilterType.InstructionTypes, filterOption.ClsdHideAssocMember.ToUpper() == "Y");
                                           break;
                                       default:
                                           if (logger.IsInfoEnabled)
                                           {
                                               logger.Info("Invalid entry in CATALOG.SEARCH.DEFAULTS " + filterOption.ClsdSearchElementAssocMember);
                                           }
                                           break;
                                   }
                               }
                           }
                       }
                   }
                   return result;

               });

            return configuration;
        }

        /// <summary>
        /// Retrieves the configuration information needed for registration processing asynchronously.
        /// </summary>
        public async Task<RegistrationConfiguration> GetRegistrationConfigurationAsync()
        {
            RegistrationConfiguration result;
            RegistrationConfiguration configuration = await GetOrAddToCacheAsync<RegistrationConfiguration>("RegistrationConfiguration",
              async () =>
              {
                  Ellucian.Colleague.Data.Student.DataContracts.RegDefaults regDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.RegDefaults>("ST.PARMS", "REG.DEFAULTS");
                  Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS");
                  if (regDefaults == null)
                  {
                      var errorMessage = "Unable to access registration defaults from ST.PARMS. REG.DEFAULTS. Default values will be assumed for purpose of building registration configuration in API." + Environment.NewLine
                      + "You can build a REG.DEFAULTS record by accessing the RGPD form in Colleague UI.";
                      logger.Info(errorMessage);
                      return new RegistrationConfiguration(false, 0);
                  }
                  else
                  {
                      //Add Authorization is required only if the flag is Y or y.
                      bool requireAddAuthorization = (!string.IsNullOrEmpty(regDefaults.RgdRequireAddAuthFlag) && regDefaults.RgdRequireAddAuthFlag.ToUpper() == "Y");
                      int offsetDays = 0;
                      // Offset days are only applicable if require add authorization is true.
                      if (requireAddAuthorization)
                      {
                          offsetDays = regDefaults.RgdAddAuthStartOffset.HasValue ? regDefaults.RgdAddAuthStartOffset.Value : 0;
                      }

                       result = new RegistrationConfiguration(requireAddAuthorization, offsetDays);
                      
                     
                  }
                  if(stwebDefaults==null)
                  {
                      var errorMessage = "Unable to access registration defaults from ST.PARMS. STWEB.DEFAULTS.";
                      logger.Info(errorMessage);
                  }
                  else
                  {
                      result.PromptForDropReason = !string.IsNullOrEmpty(stwebDefaults.StwebDropRsnPromptFlag) && stwebDefaults.StwebDropRsnPromptFlag.ToUpper() == "Y" ? true : false;
                      result.RequireDropReason = !string.IsNullOrEmpty(stwebDefaults.StwebDropRsnRequiredFlag) && stwebDefaults.StwebDropRsnRequiredFlag.ToUpper() == "Y" ? true : false;
                  }
                   return result;

              });
            return configuration;
        }


        /// <summary>
        /// Get an external mapping
        /// </summary>
        /// <param name="id">External Mapping ID</param>
        /// <returns>The external mapping info</returns>
        private async Task<ExternalMapping> GetExternalMappingAsync(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                ElfTranslateTables elfTranslateTable = await DataReader.ReadRecordAsync<ElfTranslateTables>(id);
                if (elfTranslateTable == null)
                {
                    throw new ArgumentOutOfRangeException("External Mapping ID " + id + " is not valid.");
                }

                return BuildExternalMapping(elfTranslateTable);
            }
            return null;
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
                    LogDataError("External mapping", table.Recordkey, table, ex);
                }
            }
            return null;
        }

        /// <summary>
        /// Builds Course CDM configuration
        /// </summary>
        /// <returns>Course CDM configuration</returns>
        private async Task<CurriculumConfiguration> BuildCurriculumConfigurationAsync()
        {
            LdmDefaults ldmDefaults = await GetLdmDefaultsAsync();
            CdDefaults cdDefaults = await GetCdDefaultsAsync();

            ExternalMapping subjectDepartmentMapping = await GetExternalMappingAsync(ldmDefaults.LdmdSubjDeptMapping);

            string approverId = null;
            string approvingAgencyId = null;
            string defaultCourseCreditTypeCode = null;
            string defaultAcademicLevelCode = null;
            string defaultCourseLevelCode = null;
            string defaultTeachingArrangement = null;
            string defaultContractType = null;
            string defaultContractPosition = null;
            string defaultContractLoadPeriod = null;

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
                        case "CSF.TEACHING.ARRANGEMENT":
                            defaultTeachingArrangement = c.LdmdCollDefaultValueAssocMember;
                            break;
                        case "PAC.TYPE":
                            defaultContractType = c.LdmdCollDefaultValueAssocMember;
                            break;
                        case "PLPP.POSITION.ID":
                            defaultContractPosition = c.LdmdCollDefaultValueAssocMember;
                            break;
                        case "PLP.LOAD.PERIOD":
                            defaultContractLoadPeriod = c.LdmdCollDefaultValueAssocMember;
                            break;
                        default:
                            break;
                    }
                }
            }

            return new CurriculumConfiguration()
            {
                SubjectDepartmentMapping = subjectDepartmentMapping,
                CourseActiveStatusCode = ldmDefaults.LdmdCourseActStatus,
                CourseInactiveStatusCode = ldmDefaults.LdmdCourseInactStatus,
                SectionActiveStatusCode = ldmDefaults.LdmdSectionActStatus,
                SectionInactiveStatusCode = ldmDefaults.LdmdSectionInactStatus,
                ApprovingAgencyId = approvingAgencyId,
                ApproverId = approverId,
                DefaultCreditTypeCode = defaultCourseCreditTypeCode,
                DefaultInstructionalMethodCode = cdDefaults.CdInstrMethods,
                DefaultAcademicLevelCode = defaultAcademicLevelCode,
                DefaultCourseLevelCode = defaultCourseLevelCode,
                CourseDelimiter = cdDefaults.CdCourseDelimiter,
                WaitlistRatingCode = cdDefaults.CdWaitlistRating,
                AllowAudit = cdDefaults.CdAllowAuditFlag == "Y",
                AllowPassNoPass = cdDefaults.CdAllowPassNopassFlag == "Y",
                AllowWaitlist = cdDefaults.CdAllowWaitlistFlag == "Y",
                OnlyPassNoPass = cdDefaults.CdOnlyPassNopassFlag == "Y",
                IsInstructorConsentRequired = cdDefaults.CdFacultyConsentFlag == "Y",
                AreRequisitesConverted = cdDefaults.CdReqsConvertedFlag == "Y",
                DefaultTeachingArrangement = defaultTeachingArrangement,
                DefaultContractType = defaultContractType,
                DefaultContractPosition = defaultContractPosition,
                DefaultContractLoadPeriod = defaultContractLoadPeriod
            };
        }

        private async Task<LdmDefaults> GetLdmDefaultsAsync()
        {
            LdmDefaults ldmDefaults = await DataReader.ReadRecordAsync<LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS");

            if (ldmDefaults == null)
            {
                // LDM.DEFAULTS must exist for CDM configuration to function properly
                throw new ConfigurationException("CDM configuration setup not complete.");
            }
            return ldmDefaults;
        }

        private async Task<CdDefaults> GetCdDefaultsAsync()
        {
            CdDefaults cdDefaults = await DataReader.ReadRecordAsync<CdDefaults>("ST.PARMS", "CD.DEFAULTS");

            if (cdDefaults == null)
            {
                // CD.DEFAULTS must exist for CDM configuration to function properly
                throw new ConfigurationException("Course Default configuration setup not complete.");
            }
            return cdDefaults;
        }

        private GraduationConfiguration BuildGraduationConfiguration(StwebDefaults webDefaults, Collection<Ellucian.Colleague.Data.Student.DataContracts.GraduationQuestions> graduationQuestions, string defaultEmailType)
        {
            GraduationConfiguration configuration = new GraduationConfiguration();
            if (webDefaults != null)
            {
                if (webDefaults.StwebGradTerms != null)
                {
                    // If the client has not defined any graduation terms allowed then the graduation application process is effectively not available, but just returning an empty list.
                    foreach (var termCode in webDefaults.StwebGradTerms)
                    {
                        if (!string.IsNullOrEmpty(termCode))
                        {
                            configuration.AddGraduationTerm(termCode);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(webDefaults.StwebGradCommencementUrl))
                {
                    configuration.CommencementInformationLink = webDefaults.StwebGradCommencementUrl.Replace(DmiString.sVM, string.Empty);
                    configuration.CommencementInformationLink = Regex.Replace(configuration.CommencementInformationLink, @"\s+", "");
                }
                if (!string.IsNullOrEmpty(webDefaults.StwebGradCapgownUrl))
                {
                    configuration.CapAndGownLink = webDefaults.StwebGradCapgownUrl.Replace(DmiString.sVM, string.Empty);
                    configuration.CapAndGownLink = Regex.Replace(configuration.CapAndGownLink, @"\s+", "");
                }
                if (!string.IsNullOrEmpty(webDefaults.StwebGradPhoneticUrl))
                {
                    configuration.PhoneticSpellingLink = webDefaults.StwebGradPhoneticUrl.Replace(DmiString.sVM, string.Empty);
                    configuration.PhoneticSpellingLink = Regex.Replace(configuration.PhoneticSpellingLink, @"\s+", "");
                }
                if (!string.IsNullOrEmpty(webDefaults.StwebGradDiffProgramUrl))
                {
                    configuration.ApplyForDifferentProgramLink = webDefaults.StwebGradDiffProgramUrl.Replace(DmiString.sVM, string.Empty);
                    configuration.ApplyForDifferentProgramLink = Regex.Replace(configuration.ApplyForDifferentProgramLink, @"\s+", "");
                }
                if (!string.IsNullOrEmpty(webDefaults.StwebGradCapgownSizesUrl))
                {
                    configuration.CapAndGownSizingLink = webDefaults.StwebGradCapgownSizesUrl.Replace(DmiString.sVM, string.Empty);
                    configuration.CapAndGownSizingLink = Regex.Replace(configuration.CapAndGownSizingLink, @"\s+", "");
                }
                configuration.OverrideCapAndGownDisplay = !string.IsNullOrEmpty(webDefaults.StwebGradOvrCmcmtCapgown) && (webDefaults.StwebGradOvrCmcmtCapgown.ToUpper() == "Y");
                configuration.MaximumCommencementGuests = (webDefaults.StwebGradMaxGuests == null) ? 100 : webDefaults.StwebGradMaxGuests;

                configuration.RequireImmediatePayment = webDefaults.StwebGradRequirePayment != null && webDefaults.StwebGradRequirePayment.ToUpper() == "Y";
            }

            // These optional questions will ONLY appear on the graduation application if they are retrieved from Colleague and their "HIDE" flag is not set to Y.
            if (graduationQuestions != null && graduationQuestions.Count > 0)
            {
                foreach (var question in graduationQuestions)
                {
                    if (question.GradqHide.ToUpper() != "Y")
                    {
                        switch (question.Recordkey.ToUpper())
                        {
                            case "DIPLOMA_NAME":
                                configuration.AddGraduationQuestion(GraduationQuestionType.DiplomaName, question.GradqIsRequired.ToUpper() == "Y");
                                break;
                            case "PHONETIC_SPELLING":
                                configuration.AddGraduationQuestion(GraduationQuestionType.PhoneticSpelling, question.GradqIsRequired.ToUpper() == "Y");
                                break;
                            case "HOMETOWN":
                                configuration.AddGraduationQuestion(GraduationQuestionType.Hometown, question.GradqIsRequired.ToUpper() == "Y");
                                break;
                            case "ATTEND_COMMENCEMENT":
                                configuration.AddGraduationQuestion(GraduationQuestionType.AttendCommencement, question.GradqIsRequired.ToUpper() == "Y");
                                break;
                            case "COMMENCEMENT_LOCATION":
                                configuration.AddGraduationQuestion(GraduationQuestionType.CommencementLocation, question.GradqIsRequired.ToUpper() == "Y");
                                break;
                            case "PICKUP_DIPLOMA":
                                configuration.AddGraduationQuestion(GraduationQuestionType.PickUpDiploma, question.GradqIsRequired.ToUpper() == "Y");
                                break;
                            case "NAME_IN_PROGRAM":
                                configuration.AddGraduationQuestion(GraduationQuestionType.NameInProgram, question.GradqIsRequired.ToUpper() == "Y");
                                break;
                            case "NUMBER_GUESTS":
                                configuration.AddGraduationQuestion(GraduationQuestionType.NumberGuests, question.GradqIsRequired.ToUpper() == "Y");
                                break;
                            case "CAP_SIZE":
                                configuration.AddGraduationQuestion(GraduationQuestionType.CapSize, question.GradqIsRequired.ToUpper() == "Y");
                                break;
                            case "GOWN_SIZE":
                                configuration.AddGraduationQuestion(GraduationQuestionType.GownSize, question.GradqIsRequired.ToUpper() == "Y");
                                break;
                            case "MILITARY_STATUS":
                                configuration.AddGraduationQuestion(GraduationQuestionType.MilitaryStatus, question.GradqIsRequired.ToUpper() == "Y");
                                break;
                            case "SPECIAL_ACCOMMODATIONS":
                                configuration.AddGraduationQuestion(GraduationQuestionType.SpecialAccommodations, question.GradqIsRequired.ToUpper() == "Y");
                                break;
                            case "PRIMARY_LOCATION":
                                configuration.AddGraduationQuestion(GraduationQuestionType.PrimaryLocation, question.GradqIsRequired.ToUpper() == "Y");
                                break;
                            case "REQUEST_ADDRESS_CHANGE":
                                configuration.AddGraduationQuestion(GraduationQuestionType.RequestAddressChange, question.GradqIsRequired.ToUpper() == "Y");
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            configuration.DefaultWebEmailType = defaultEmailType;
            configuration.EmailGradNotifyPara = webDefaults.StwebGradNotifyPara;
            return configuration;
        }

        /// <summary>
        /// Build the Student Request Configuration
        /// </summary>
        /// <param name="webDefaults">StWebDefault record</param>
        /// <param name="defaultEmailType">Default email type to use</param>
        /// <returns></returns>
        private StudentRequestConfiguration BuildStudentRequestConfiguration(StwebDefaults webDefaults, string defaultEmailType)
        {
            StudentRequestConfiguration configuration = new StudentRequestConfiguration();
            configuration.DefaultWebEmailType = defaultEmailType;
            if (webDefaults != null)
            {
                configuration.SendTranscriptRequestConfirmation = !string.IsNullOrEmpty(webDefaults.StwebTranNotifyPara);
                configuration.SendEnrollmentRequestConfirmation = !string.IsNullOrEmpty(webDefaults.StwebEnrlNotifyPara);
                configuration.TranscriptRequestRequireImmediatePayment = !string.IsNullOrEmpty(webDefaults.StwebTranRequirePayment) && webDefaults.StwebTranRequirePayment.Equals("Y", StringComparison.OrdinalIgnoreCase);
                configuration.EnrollmentRequestRequireImmediatePayment = !string.IsNullOrEmpty(webDefaults.StwebEnrlRequirePayment) && webDefaults.StwebEnrlRequirePayment.Equals("Y", StringComparison.OrdinalIgnoreCase);
            }
            return configuration;
        }

        /// <summary>
        /// Build the Faculty Grading Configuration
        /// </summary>
        /// <param name="webDefaults">StWebDefault record</param>
        /// <returns>FacultyGradingConfiguration</returns>
        private async Task<FacultyGradingConfiguration> BuildFacultyGradingConfigurationAsync(StwebDefaults webDefaults)
        {
            FacultyGradingConfiguration configuration = new FacultyGradingConfiguration();
            if (webDefaults != null)
            {
                configuration.IncludeCrosslistedStudents = !string.IsNullOrEmpty(webDefaults.StwebGradeInclXlist) && (webDefaults.StwebGradeInclXlist.ToUpper() == "Y");
                configuration.IncludeDroppedWithdrawnStudents = !string.IsNullOrEmpty(webDefaults.StwebGradeDropsFlag) && (webDefaults.StwebGradeDropsFlag.ToUpper() == "Y");
            }
            var allowedGradingTerms = await GetAllowedGradingTermsAsync();
            if (allowedGradingTerms != null && allowedGradingTerms.Any())
            {
                foreach (var term in allowedGradingTerms)
                {
                    try
                    {
                        configuration.AddGradingTerm(term.Code);
                    }
                    catch (Exception)
                    {
                        // Ignore duplicates.
                    }

                }
            }
            configuration.VerifyGrades = !string.IsNullOrEmpty(webDefaults.StwebVerifyGrades) ? webDefaults.StwebVerifyGrades.Equals("Y", StringComparison.OrdinalIgnoreCase) ? true : false : default(bool?);
            // Catch and log exception if the either the STWEB.MIDTERM.GRADE.COUNT is not a number or if it isn't between 0 - 6. But continue either way and let it default to zero. 
            if (!string.IsNullOrEmpty(webDefaults.StwebMidtermGradeCount))
            {
                try
                {
                    int midtermGradeCount;
                    if (int.TryParse(webDefaults.StwebMidtermGradeCount, out midtermGradeCount))
                    {
                        configuration.NumberOfMidtermGrades = midtermGradeCount;
                    }
                    else
                    {
                        logger.Info("Unable to convert STWEB.MIDTERM.GRADE.COUNT " + webDefaults.StwebMidtermGradeCount + "to an integer");
                    }

                }
                catch (Exception ex)
                {
                    logger.Info(ex.Message);
                }
            }
            return configuration;
        }

        private async Task<IEnumerable<GradingTerm>> GetAllowedGradingTermsAsync()
        {
            return await GetValcodeAsync<GradingTerm>("ST", "GRADING.TERMS", r =>
                (new GradingTerm(r.ValInternalCodeAssocMember, r.ValExternalRepresentationAssocMember)), Level1CacheTimeoutValue);
        }

    }
}
