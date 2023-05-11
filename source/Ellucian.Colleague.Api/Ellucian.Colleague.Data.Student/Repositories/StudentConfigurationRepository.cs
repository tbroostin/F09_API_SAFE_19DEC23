// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
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
    public class StudentConfigurationRepository : BaseStudentRepository, IStudentConfigurationRepository
    {
        private const string _preferredHierarchyCode = "PREFERRED";
        private static char _VM = Convert.ToChar(DynamicArray.VM);
        public StudentConfigurationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        public async Task<StudentConfiguration> GetStudentConfigurationAsync()
        {
            try
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
                            throw new ColleagueWebApiException(errorMessage);
                        }
                        studentConfig.FacultyEmailTypeCode = stwebDefaults.StwebProfileFacEmailType;
                        studentConfig.FacultyPhoneTypeCode = stwebDefaults.StwebProfileFacPhoneType;
                        studentConfig.EnforceTranscriptRestriction = stwebDefaults.StwebTranVerifyRestr.Equals("Y", StringComparison.OrdinalIgnoreCase) ? true : false;
                        return studentConfig;
                    });
                return studentConfiguration;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while getting student configuration");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }
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
        /// Retrieve Course Delimiter
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetCourseDelimiterAsync()
        {
            return await GetOrAddToCacheAsync<string>("CourseDelimiter",
                async () =>
                {
                    CdDefaults cdDefaults = await GetCdDefaultsAsync();
                    return cdDefaults.CdCourseDelimiter;
                });
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
                   var daDefaultsTask = DataReader.ReadRecordAsync<Data.Student.DataContracts.DaDefaults>("ST.PARMS", "DA.DEFAULTS");
                   var valCodesTask = DataReader.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.PROGRAM.STATUSES");

                   await Task.WhenAll(stwebDefaultsTask, graduationQuestionsTask, defaultsTask, daDefaultsTask, valCodesTask);
                   Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = stwebDefaultsTask.Result;
                   Collection<Ellucian.Colleague.Data.Student.DataContracts.GraduationQuestions> graduationQuestions = graduationQuestionsTask.Result;
                   Ellucian.Colleague.Data.Base.DataContracts.Defaults defaultData = defaultsTask.Result;
                   Data.Student.DataContracts.DaDefaults daDefaults = daDefaultsTask.Result;
                   ApplValcodes valCodes = valCodesTask.Result;

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
                       throw new ColleagueWebApiException(errorMessage);
                   }
                   if (daDefaults == null)
                   {
                       var errorMessage = "Unable to access DA.DEFAULTS from ST.PARMS table.";
                       logger.Info(errorMessage);
                       throw new ColleagueWebApiException(errorMessage);
                   }
                   return BuildGraduationConfiguration(stwebDefaults, graduationQuestions, defaultData.DefaultWebEmailType, daDefaults, valCodes);
               });

            return studentConfiguration;
        }

        /// <summary>
        /// Get the student request configuration information needed for a new transcript request or enrollment request
        /// </summary>
        /// <returns>The StudentRequestConfiguration entity</returns>
        public async Task<StudentRequestConfiguration> GetStudentRequestConfigurationAsync()
        {
            try
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
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving student request configuration information.";
                logger.Error(tex, message);
                throw;
            }
            catch (Exception ex)
            {
                string message = "An exception occurred while retrieving student request configuration information.";
                logger.Error(ex, message);
                throw;
            }
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
                   Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults2 stwebDefaults2 = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults2>("ST.PARMS", "STWEB.DEFAULTS.2");

                   if (stwebDefaults == null)
                   {
                       var errorMessage = "Unable to access student web defaults from ST.PARMS. STWEB.DEFAULTS.";
                       logger.Info(errorMessage);
                       stwebDefaults = new StwebDefaults();
                   }


                   if (stwebDefaults2 == null)
                   {
                       var errorMessage = "Unable to access student web defaults from ST.PARMS. STWEB.DEFAULTS.2";
                       logger.Info(errorMessage);
                       stwebDefaults2 = new StwebDefaults2();
                   }
                   return await BuildFacultyGradingConfigurationAsync(stwebDefaults, stwebDefaults2);
               });

            return configuration;
        }

        /// <summary>
        /// Get the faculty grading configuration information
        /// </summary>
        /// <returns>The FacultyGradingConfiguration2 entity</returns>
        public async Task<FacultyGradingConfiguration2> GetFacultyGradingConfiguration2Async()
        {
            try
            {
                FacultyGradingConfiguration2 configuration2 = await GetOrAddToCacheAsync<FacultyGradingConfiguration2>("FacultyGradingConfiguration2",
                   async () =>
                   {
                       Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS");
                       Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults2 stwebDefaults2 = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults2>("ST.PARMS", "STWEB.DEFAULTS.2");

                       if (stwebDefaults == null)
                       {
                           var errorMessage = "Unable to access student web defaults from ST.PARMS. STWEB.DEFAULTS.";
                           logger.Info(errorMessage);
                           stwebDefaults = new StwebDefaults();
                       }


                       if (stwebDefaults2 == null)
                       {
                           var errorMessage = "Unable to access student web defaults from ST.PARMS. STWEB.DEFAULTS.2";
                           logger.Info(errorMessage);
                           stwebDefaults2 = new StwebDefaults2();
                       }
                       return await BuildFacultyGradingConfiguration2Async(stwebDefaults, stwebDefaults2);
                   });

                return configuration2;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while retrieving faculty grading configuration information.");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Get the student profile configurations
        /// </summary>
        /// <returns>The StudentProfileConfiguration entity</returns>
        public async Task<StudentProfileConfiguration> GetStudentProfileConfigurationAsync()
        {
            StudentProfileConfiguration result = new StudentProfileConfiguration();
            StudentProfileConfiguration configuration = await GetOrAddToCacheAsync<StudentProfileConfiguration>("StudentProfileConfiguration",
           async () =>
           {
               StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS");

               if (stwebDefaults == null)
               {
                   var errorMessage = "Unable to access student web defaults from ST.PARMS. STWEB.DEFAULTS.";
                   logger.Info(errorMessage);
                   stwebDefaults = new StwebDefaults();
               }
               else
               {
                   result.FacultyPersonConfiguration = new StudentProfilePersonConfiguration()
                   {
                       ShowAcadamicPrograms = (!string.IsNullOrEmpty(stwebDefaults.StwebStShowAcadProgFac) && stwebDefaults.StwebStShowAcadProgFac.ToUpper() == "Y"),
                       ShowPhone = (!string.IsNullOrEmpty(stwebDefaults.StwebStShowPhoneFac) && stwebDefaults.StwebStShowPhoneFac.ToUpper() == "Y"),
                       ShowAcadLevelStanding = (!string.IsNullOrEmpty(stwebDefaults.StwebStShowAcadStndFac) && stwebDefaults.StwebStShowAcadStndFac.ToUpper() == "Y"),
                       ShowAddress = (!string.IsNullOrEmpty(stwebDefaults.StwebStShowAddressFac) && stwebDefaults.StwebStShowAddressFac.ToUpper() == "Y"),
                       ShowAdvisorDetails = (!string.IsNullOrEmpty(stwebDefaults.StwebStShowAdvsorDtlFac) && stwebDefaults.StwebStShowAdvsorDtlFac.ToUpper() == "Y"),
                       ShowAdvisorOfficeHours = (!string.IsNullOrEmpty(stwebDefaults.StwebStShowAdvOfcHrFac) && stwebDefaults.StwebStShowAdvOfcHrFac.ToUpper() == "Y"),
                       ShowAnticipatedCompletionDate = (!string.IsNullOrEmpty(stwebDefaults.StwebStShowAncptDateFac) && stwebDefaults.StwebStShowAncptDateFac.ToUpper() == "Y")
                   };

                   result.AdvisorPersonConfiguration = new StudentProfilePersonConfiguration()
                   {
                       ShowAcadamicPrograms = (!string.IsNullOrEmpty(stwebDefaults.StwebStShowAcadProgAdv) && stwebDefaults.StwebStShowAcadProgAdv.ToUpper() == "Y"),
                       ShowPhone = (!string.IsNullOrEmpty(stwebDefaults.StwebStShowPhoneAdv) && stwebDefaults.StwebStShowPhoneAdv.ToUpper() == "Y"),
                       ShowAcadLevelStanding = (!string.IsNullOrEmpty(stwebDefaults.StwebStShowAcadStndAdv) && stwebDefaults.StwebStShowAcadStndAdv.ToUpper() == "Y"),
                       ShowAddress = (!string.IsNullOrEmpty(stwebDefaults.StwebStShowAddressAdv) && stwebDefaults.StwebStShowAddressAdv.ToUpper() == "Y"),
                       ShowAdvisorDetails = (!string.IsNullOrEmpty(stwebDefaults.StwebStShowAdvsorDtlAdv) && stwebDefaults.StwebStShowAdvsorDtlAdv.ToUpper() == "Y"),
                       ShowAdvisorOfficeHours = (!string.IsNullOrEmpty(stwebDefaults.StwebStShowAdvOfcHrAdv) && stwebDefaults.StwebStShowAdvOfcHrAdv.ToUpper() == "Y"),
                       ShowAnticipatedCompletionDate = (!string.IsNullOrEmpty(stwebDefaults.StwebStShowAncptDateAdv) && stwebDefaults.StwebStShowAncptDateAdv.ToUpper() == "Y")
                   };

                   result.PhoneTypesHierarchy = stwebDefaults.StwebProfilePhoneType;
                   result.EmailTypesHierarchy = stwebDefaults.StwebProfileEmailType;
                   result.ProfileFacultyEmailType = stwebDefaults.StwebProfileFacEmailType;
                   result.ProfileFacultyPhoneType = stwebDefaults.StwebProfileFacPhoneType;
                   result.ProfileAdvsiorType = stwebDefaults.StwebProfileAdvType;
                   result.IsDisplayOfficeHours = (!string.IsNullOrEmpty(stwebDefaults.StwebDisplayOfficeHours) && stwebDefaults.StwebDisplayOfficeHours.ToUpper() == "Y");
                   NameAddressHierarchy addresshierarchy = await GetAddressHierarchyAsync(stwebDefaults.StwebProfileStuAddrHier);
                   result.AddressTypesHierarchy = new List<string>();
                   if (addresshierarchy != null && addresshierarchy.AddressTypeHierarchy != null && addresshierarchy.AddressTypeHierarchy.Any())
                   {
                       foreach (var addressType in addresshierarchy.AddressTypeHierarchy)
                       {
                           result.AddressTypesHierarchy.Add(addressType);
                       }
                   }
               }
               return result;
           });
            return configuration;
        }

        /// <summary>
        /// Get the course catalog configuration information needed for course catalog searches
        /// </summary>
        /// <returns>The CourseCatalogConfiguration entity</returns>
        [Obsolete("Obsolete as of API version 1.26. Use GetCourseCatalogConfiguration2Async.")]
        public async Task<CourseCatalogConfiguration> GetCourseCatalogConfigurationAsync()
        {
            CourseCatalogConfiguration configuration = await GetOrAddToCacheAsync<CourseCatalogConfiguration>("CourseCatalogConfiguration",
               async () =>
               {
                   Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS");
                   Ellucian.Colleague.Data.Student.DataContracts.CatalogSearchDefaults catalogSearchDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.CatalogSearchDefaults>("ST.PARMS", "CATALOG.SEARCH.DEFAULTS");
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
                       bool showCatSecBookInformation = !string.IsNullOrEmpty(stwebDefaults.StwebShowBookInformation) && stwebDefaults.StwebShowBookInformation.ToUpper() == "Y" ? true : false;

                       result = new CourseCatalogConfiguration(stwebDefaults.StwebRegStartDate, stwebDefaults.StwebRegEndDate)
                       {
                           ShowCourseSectionFeeInformation = showCatSecOtherFees,
                           ShowCourseSectionBookInformation = showCatSecBookInformation
                       };
                   }

                   if (catalogSearchDefaults == null)
                   {
                       var errorMessage = "Unable to access catalog search defaults from ST.PARMS: CATALOG.SEARCH.DEFAULTS.";
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
        /// Get the course catalog configuration information needed for course catalog searches
        /// </summary>
        /// <returns>The CourseCatalogConfiguration2 entity</returns>
        [Obsolete("Obsolete. Use GetCourseCatalogConfiguration3Async.")]
        public async Task<CourseCatalogConfiguration> GetCourseCatalogConfiguration2Async()
        {
            CourseCatalogConfiguration configuration = await GetOrAddToCacheAsync<CourseCatalogConfiguration>("CourseCatalogConfiguration2",
               async () =>
               {
                   Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS");
                   Ellucian.Colleague.Data.Student.DataContracts.CatalogSearchDefaults catalogSearchDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.CatalogSearchDefaults>("ST.PARMS", "CATALOG.SEARCH.DEFAULTS");
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
                       bool showCatSecBookInformation = !string.IsNullOrEmpty(stwebDefaults.StwebShowBookInformation) && stwebDefaults.StwebShowBookInformation.ToUpper() == "Y" ? true : false;
                       result = new CourseCatalogConfiguration(stwebDefaults.StwebRegStartDate, stwebDefaults.StwebRegEndDate)
                       {
                           ShowCourseSectionFeeInformation = showCatSecOtherFees,
                           ShowCourseSectionBookInformation = showCatSecBookInformation
                       };
                   }

                   if (catalogSearchDefaults == null)
                   {
                       var errorMessage = "Unable to access catalog search defaults from ST.PARMS: CATALOG.SEARCH.DEFAULTS.";
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
                                       case "SYNONYMS":
                                           result.AddCatalogFilterOption(CatalogFilterType.Synonyms, filterOption.ClsdHideAssocMember.ToUpper() == "Y");
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
        /// Get the course catalog configuration information needed for course catalog searches
        /// </summary>
        /// <returns>The CourseCatalogConfiguration3 entity</returns>
        [Obsolete("Obsolete as of API version 1.32. Use GetCourseCatalogConfiguration4Async.")]
        public async Task<CourseCatalogConfiguration> GetCourseCatalogConfiguration3Async()
        {
            CourseCatalogConfiguration configuration = await GetOrAddToCacheAsync<CourseCatalogConfiguration>("CourseCatalogConfiguration3",
               async () =>
               {
                   Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS");
                   Ellucian.Colleague.Data.Student.DataContracts.CatalogSearchDefaults catalogSearchDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.CatalogSearchDefaults>("ST.PARMS", "CATALOG.SEARCH.DEFAULTS");
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
                       bool showCatSecBookInformation = !string.IsNullOrEmpty(stwebDefaults.StwebShowBookInformation) && stwebDefaults.StwebShowBookInformation.ToUpper() == "Y" ? true : false;
                       SelfServiceCourseCatalogSearchView defaultSearchView = SelfServiceCourseCatalogSearchView.SubjectSearch;
                       if (!string.IsNullOrEmpty(stwebDefaults.StwebUseAdvSearchByDflt) && stwebDefaults.StwebUseAdvSearchByDflt.ToUpper() == "Y")
                       {
                           defaultSearchView = SelfServiceCourseCatalogSearchView.AdvancedSearch;
                       }

                       //override the default view of the course catalog search results to be the section table view when StwebSrchRsltTblVwFlag is yes
                       SelfServiceCourseCatalogSearchResultView defaultSearchResultView = SelfServiceCourseCatalogSearchResultView.CatalogListing;
                       if (!string.IsNullOrEmpty(stwebDefaults.StwebSrchRsltTblVwFlag) && stwebDefaults.StwebSrchRsltTblVwFlag.ToUpper() == "Y")
                       {
                           defaultSearchResultView = SelfServiceCourseCatalogSearchResultView.SectionListing;
                       }

                       result = new CourseCatalogConfiguration(stwebDefaults.StwebRegStartDate, stwebDefaults.StwebRegEndDate)
                       {
                           ShowCourseSectionFeeInformation = showCatSecOtherFees,
                           ShowCourseSectionBookInformation = showCatSecBookInformation,
                           DefaultSelfServiceCourseCatalogSearchView = defaultSearchView,
                           DefaultSelfServiceCourseCatalogSearchResultView = defaultSearchResultView
                       };
                   }

                   if (catalogSearchDefaults == null)
                   {
                       var errorMessage = "Unable to access catalog search defaults from ST.PARMS: CATALOG.SEARCH.DEFAULTS.";
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
                                       case "TIME_STARTS_ENDS":
                                           result.AddCatalogFilterOption(CatalogFilterType.TimeStartsEnds, filterOption.ClsdHideAssocMember.ToUpper() == "Y");
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
                                       case "SYNONYMS":
                                           result.AddCatalogFilterOption(CatalogFilterType.Synonyms, filterOption.ClsdHideAssocMember.ToUpper() == "Y");
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

                       //setup course catalog search result header options
                       if (catalogSearchDefaults.ClsdAdvSearchResultsEntityAssociation != null && catalogSearchDefaults.ClsdAdvSearchResultsEntityAssociation.Any())
                       {
                           foreach (var headerOption in catalogSearchDefaults.ClsdAdvSearchResultsEntityAssociation)
                           {
                               if (headerOption.ClsdAdvSearchElementsAssocMember != null)
                               {
                                   switch (headerOption.ClsdAdvSearchElementsAssocMember.ToUpper())
                                   {
                                       case "ACADEMIC.LEVEL":
                                           result.AddCatalogSearchResultHeaderOption(CatalogSearchResultHeaderType.AcademicLevel, headerOption.ClsdAdvSearchHideAssocMember.ToUpper() == "Y");
                                           break;
                                       case "LOCATION":
                                           result.AddCatalogSearchResultHeaderOption(CatalogSearchResultHeaderType.Location, headerOption.ClsdAdvSearchHideAssocMember.ToUpper() == "Y");
                                           break;
                                       case "PLANNED.STATUS":
                                           result.AddCatalogSearchResultHeaderOption(CatalogSearchResultHeaderType.PlannedStatus, headerOption.ClsdAdvSearchHideAssocMember.ToUpper() == "Y");
                                           break;

                                       default:
                                           if (logger.IsInfoEnabled)
                                           {
                                               logger.Info("Invalid entry in CATALOG.SEARCH.DEFAULTS -> CLSD.ADV.SEARCH.HIDE property" + headerOption.ClsdAdvSearchHideAssocMember);
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
        /// Get the course catalog configuration information needed for course catalog searches
        /// </summary>
        /// <returns>The CourseCatalogConfiguration entity</returns>
        public async Task<CourseCatalogConfiguration> GetCourseCatalogConfiguration4Async()
        {
            CourseCatalogConfiguration configuration = await GetOrAddToCacheAsync<CourseCatalogConfiguration>("CourseCatalogConfiguration",
               async () =>
               {
                   Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS");
                   Ellucian.Colleague.Data.Student.DataContracts.CatalogSearchDefaults catalogSearchDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.CatalogSearchDefaults>("ST.PARMS", "CATALOG.SEARCH.DEFAULTS");
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
                       bool showCatSecBookInformation = !string.IsNullOrEmpty(stwebDefaults.StwebShowBookInformation) && stwebDefaults.StwebShowBookInformation.ToUpper() == "Y" ? true : false;
                       SelfServiceCourseCatalogSearchView defaultSearchView = SelfServiceCourseCatalogSearchView.SubjectSearch;
                       if (!string.IsNullOrEmpty(stwebDefaults.StwebUseAdvSearchByDflt) && stwebDefaults.StwebUseAdvSearchByDflt.ToUpper() == "Y")
                       {
                           defaultSearchView = SelfServiceCourseCatalogSearchView.AdvancedSearch;
                       }

                       //override the default view of the course catalog search results to be the section table view when StwebSrchRsltTblVwFlag is yes
                       SelfServiceCourseCatalogSearchResultView defaultSearchResultView = SelfServiceCourseCatalogSearchResultView.CatalogListing;
                       if (!string.IsNullOrEmpty(stwebDefaults.StwebSrchRsltTblVwFlag) && stwebDefaults.StwebSrchRsltTblVwFlag.ToUpper() == "Y")
                       {
                           defaultSearchResultView = SelfServiceCourseCatalogSearchResultView.SectionListing;
                       }

                       result = new CourseCatalogConfiguration(stwebDefaults.StwebRegStartDate, stwebDefaults.StwebRegEndDate)
                       {
                           ShowCourseSectionFeeInformation = showCatSecOtherFees,
                           ShowCourseSectionBookInformation = showCatSecBookInformation,
                           DefaultSelfServiceCourseCatalogSearchView = defaultSearchView,
                           DefaultSelfServiceCourseCatalogSearchResultView = defaultSearchResultView,
                       };
                   }

                   if (catalogSearchDefaults == null)
                   {
                       var errorMessage = "Unable to access catalog search defaults from ST.PARMS: CATALOG.SEARCH.DEFAULTS.";
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
                                       case "TIME_STARTS_ENDS":
                                           result.AddCatalogFilterOption(CatalogFilterType.TimeStartsEnds, filterOption.ClsdHideAssocMember.ToUpper() == "Y");
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
                                       case "SYNONYMS":
                                           result.AddCatalogFilterOption(CatalogFilterType.Synonyms, filterOption.ClsdHideAssocMember.ToUpper() == "Y");
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

                       //setup course catalog search result header options
                       if (catalogSearchDefaults.ClsdAdvSearchResultsEntityAssociation != null && catalogSearchDefaults.ClsdAdvSearchResultsEntityAssociation.Any())
                       {
                           foreach (var headerOption in catalogSearchDefaults.ClsdAdvSearchResultsEntityAssociation)
                           {
                               if (headerOption.ClsdAdvSearchElementsAssocMember != null)
                               {
                                   switch (headerOption.ClsdAdvSearchElementsAssocMember.ToUpper())
                                   {
                                       case "ACADEMIC.LEVEL":
                                           result.AddCatalogSearchResultHeaderOption(CatalogSearchResultHeaderType.AcademicLevel, headerOption.ClsdAdvSearchHideAssocMember.ToUpper() == "Y");
                                           break;
                                       case "LOCATION":
                                           result.AddCatalogSearchResultHeaderOption(CatalogSearchResultHeaderType.Location, headerOption.ClsdAdvSearchHideAssocMember.ToUpper() == "Y");
                                           break;
                                       case "PLANNED.STATUS":
                                           result.AddCatalogSearchResultHeaderOption(CatalogSearchResultHeaderType.PlannedStatus, headerOption.ClsdAdvSearchHideAssocMember.ToUpper() == "Y");
                                           break;
                                       case "INSTRUCTIONAL_METHOD":
                                           result.AddCatalogSearchResultHeaderOption(CatalogSearchResultHeaderType.InstructionalMethods, headerOption.ClsdAdvSearchHideAssocMember.ToUpper() == "Y");
                                           break;
                                       case "COURSE_TYPE":
                                           result.AddCatalogSearchResultHeaderOption(CatalogSearchResultHeaderType.CourseTypes, headerOption.ClsdAdvSearchHideAssocMember.ToUpper() == "Y");
                                           break;
                                       case "PRINTED_COMMENTS":
                                           result.AddCatalogSearchResultHeaderOption(CatalogSearchResultHeaderType.Comments, headerOption.ClsdAdvSearchHideAssocMember.ToUpper() == "Y");
                                           break;
                                       case "BOOKSTORE_LINK":
                                           result.AddCatalogSearchResultHeaderOption(CatalogSearchResultHeaderType.BookstoreLink, headerOption.ClsdAdvSearchHideAssocMember.ToUpper() == "Y");
                                           break;

                                       default:
                                           if (logger.IsInfoEnabled)
                                           {
                                               logger.Info("Invalid entry in CATALOG.SEARCH.DEFAULTS -> CLSD.ADV.SEARCH.HIDE property" + headerOption.ClsdAdvSearchHideAssocMember);
                                           }
                                           break;
                                   }
                               }
                           }
                       }

                       // Use api cached sections for availablity data when the catalogSearchDefaults parameter ClsdBypassAvailCache is either blank or no
                       result.BypassApiCacheForAvailablityData = string.IsNullOrEmpty(catalogSearchDefaults.ClsdBypassAvailCache) || catalogSearchDefaults.ClsdBypassAvailCache.ToUpper() == "N" ? false : true;
                   }
                   return result;

               });

            return configuration;
        }

        /// <summary>
        /// Returns the Address Hierarchy record for the specified hierarchy code
        /// </summary>
        /// <returns>NameAddressHierarchy</returns>
        private async Task<NameAddressHierarchy> GetAddressHierarchyAsync(string nameAddressHierarchyCode = _preferredHierarchyCode)
        {
            if (string.IsNullOrEmpty(nameAddressHierarchyCode))
            {
                nameAddressHierarchyCode = _preferredHierarchyCode;
            }
            NameAddressHierarchy requestedNameAddressHierarchy = new NameAddressHierarchy(nameAddressHierarchyCode);
            NameAddrHierarchy nameAddrHierarchy = await DataReader.ReadRecordAsync<NameAddrHierarchy>("NAME.ADDR.HIERARCHY", nameAddressHierarchyCode);
            if (nameAddrHierarchy == null)
            {
                if (nameAddressHierarchyCode != _preferredHierarchyCode)
                {
                    // for all hierarchies except PREFERRED throw an error - this means nothing will be cached 
                    var errorMessage = "Unable to find NAME.ADDR.HIERARCHY record with Id " + nameAddressHierarchyCode + ". Cache not built.";
                    logger.Info(errorMessage);
                    throw new KeyNotFoundException(errorMessage);
                }
                else
                {
                    // All clients are expected to have a PREFERRED name address hierarchy. If they don't, report it but also default one so that it is cached.
                    var errorMessage = "Unable to find NAME.ADDR.HIERARCHY record with Id " + nameAddressHierarchyCode + ". Creating a basic preferred hierarchy with PF name type.";
                    logger.Info(errorMessage);
                    // Construct a default one with the desired name type values.
                    nameAddrHierarchy = new NameAddrHierarchy();
                    nameAddrHierarchy.Recordkey = _preferredHierarchyCode;
                    nameAddrHierarchy.NahNameHierarchy = new List<string>() { "PF" };
                }

            }
            // Build the NameAddressHierarchy Entity and cache that.
            requestedNameAddressHierarchy = new NameAddressHierarchy(nameAddrHierarchy.Recordkey);
            if (nameAddrHierarchy.NahAddressHierarchy != null && nameAddrHierarchy.NahAddressHierarchy.Any())
            {
                foreach (var addressType in nameAddrHierarchy.NahAddressHierarchy)
                {
                    requestedNameAddressHierarchy.AddAddressTypeHierarchy(addressType);
                }
            }

            return requestedNameAddressHierarchy;
        }

        /// <summary>
        /// Builds Course CDM configuration
        /// </summary>
        /// <returns>Course CDM configuration</returns>
        private async Task<CurriculumConfiguration> BuildCurriculumConfigurationAsync()
        {
            LdmDefaults ldmDefaults = await GetLdmDefaultsAsync();
            CdDefaults cdDefaults = await GetCdDefaultsAsync();

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

        private GraduationConfiguration BuildGraduationConfiguration(StwebDefaults webDefaults, Collection<Ellucian.Colleague.Data.Student.DataContracts.GraduationQuestions> graduationQuestions, string defaultEmailType, Data.Student.DataContracts.DaDefaults daDefaults, ApplValcodes valCodes)
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
                configuration.PreventGraduationApplicationEdits = webDefaults.StwebAllowGradAppEdits != null && webDefaults.StwebAllowGradAppEdits.ToUpper() == "N";
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

            //check for NP CP values
            if (valCodes == null)
            {
                configuration.IsNPCPValuesSetForPrograms = false;
            }
            else
            {
                if (valCodes.ValActionCode2.Count(x => x.ToUpper() == "NP") == 1 && valCodes.ValActionCode2.Count(x => x.ToUpper() == "CP") == 1)
                {
                    configuration.IsNPCPValuesSetForPrograms = true;
                }
            }

            // Anticipated date global settings
            configuration.HideAnticipatedCompletionDate =
                    (string.IsNullOrEmpty(daDefaults.DaHideAntCmplDtInSsMp) || daDefaults.DaHideAntCmplDtInSsMp.ToUpper() == "Y") ? false : true;

            // Expand Requirements setting
            if (string.IsNullOrEmpty(daDefaults.DaExpandRequirements))
            {
                configuration.ExpandRequirementSetting = ExpandRequirementSetting.None;
                configuration.ExpandRequirements = string.Empty;
            }
            else
            {
                configuration.ExpandRequirements = daDefaults.DaExpandRequirements.ToUpperInvariant();
                switch (daDefaults.DaExpandRequirements.ToUpperInvariant())
                {
                    case "E":
                        configuration.ExpandRequirementSetting = ExpandRequirementSetting.Expand;
                        break;
                    case "C":
                        configuration.ExpandRequirementSetting = ExpandRequirementSetting.Collapse;
                        break;

                }
            }
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
        private async Task<FacultyGradingConfiguration> BuildFacultyGradingConfigurationAsync(StwebDefaults webDefaults, StwebDefaults2 webDefaults2)
        {
            FacultyGradingConfiguration configuration = new FacultyGradingConfiguration();
            if (webDefaults != null)
            {
                configuration.IncludeCrosslistedStudents = !string.IsNullOrEmpty(webDefaults.StwebGradeInclXlist) && (webDefaults.StwebGradeInclXlist.ToUpper() == "Y");
                configuration.IncludeDroppedWithdrawnStudents = !string.IsNullOrEmpty(webDefaults.StwebGradeDropsFlag) && (webDefaults.StwebGradeDropsFlag.ToUpper() == "Y");
                configuration.LimitMidtermGradingToAllowedTerms = !string.IsNullOrEmpty(webDefaults.StwebMidGradeTermsFlag) && (webDefaults.StwebMidGradeTermsFlag.ToUpper() == "Y");
                configuration.ProvideMidtermGradingCompleteFeature = !string.IsNullOrEmpty(webDefaults.StwebMidGradeCmplFlag) && (webDefaults.StwebMidGradeCmplFlag.ToUpper() == "Y");
                configuration.LockMidtermGradingWhenComplete = !string.IsNullOrEmpty(webDefaults.StwebMidGradeLockFlag) && (webDefaults.StwebMidGradeLockFlag.ToUpper() == "Y");
                configuration.FinalGradesLastDateAttendedNeverAttendedDisplayBehavior = ConvertStringToLastDateAttendedNeverAttendedFieldDisplayType(webDefaults.StwebLdaNaFinalGrading);
                configuration.MidtermGradesLastDateAttendedNeverAttendedDisplayBehavior = ConvertStringToLastDateAttendedNeverAttendedFieldDisplayType(webDefaults.StwebLdaNaMidtermGrading);
                configuration.RequireLastDateAttendedOrNeverAttendedFlagBeforeFacultyDrop = !string.IsNullOrEmpty(webDefaults.StwebRequireLdanaFacDrop) && (webDefaults.StwebRequireLdanaFacDrop.ToUpper() == "Y");
                configuration.ShowPassAudit = !string.IsNullOrEmpty(webDefaults.StwebShowPassAudit) && (webDefaults.StwebShowPassAudit.ToUpper() == "Y");
                configuration.ShowRepeated = !string.IsNullOrEmpty(webDefaults.StwebShowRepeated) && (webDefaults.StwebShowRepeated.ToUpper() == "Y");
            }
            if (webDefaults2 != null)
            {
                configuration.IsGradingAllowedForDroppedWithdrawnStudents = (!string.IsNullOrEmpty(webDefaults2.Stweb2DisallowGrdeDrpWth) && webDefaults2.Stweb2DisallowGrdeDrpWth.ToUpper() == "Y") ? false : true;//On GRWP if the flag(DisallowGradingForDroppedWithdrawn) is Yes then it means faculty is not allowed to grade
                configuration.IsGradingAllowedForNeverAttendedStudents = (!string.IsNullOrEmpty(webDefaults2.Stweb2DisallowGrdeNvrAtd) && webDefaults2.Stweb2DisallowGrdeNvrAtd.ToUpper() == "Y") ? false : true;//On GRWP if the flag(DisallowGradingForNeverAttended) is Yes then it means faculty is not allowed to grade
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
                    catch (Exception ex)
                    {
                        // Ignore duplicates.
                        logger.Error(ex, "Duplicate grading term.");
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

        /// <summary>
        /// Build the Faculty Grading Configuration2
        /// </summary>
        /// <param name="webDefaults">StWebDefault record</param>
        /// <returns>FacultyGradingConfiguration2</returns>
        private async Task<FacultyGradingConfiguration2> BuildFacultyGradingConfiguration2Async(StwebDefaults webDefaults, StwebDefaults2 webDefaults2)
        {
            FacultyGradingConfiguration2 configuration2 = new FacultyGradingConfiguration2();
            if (webDefaults != null)
            {
                configuration2.IncludeCrosslistedStudents = !string.IsNullOrEmpty(webDefaults.StwebGradeInclXlist) && (webDefaults.StwebGradeInclXlist.ToUpper() == "Y");
                configuration2.IncludeDroppedWithdrawnStudents = !string.IsNullOrEmpty(webDefaults.StwebGradeDropsFlag) && (webDefaults.StwebGradeDropsFlag.ToUpper() == "Y");
                configuration2.LimitMidtermGradingToAllowedTerms = !string.IsNullOrEmpty(webDefaults.StwebMidGradeTermsFlag) && (webDefaults.StwebMidGradeTermsFlag.ToUpper() == "Y");
                configuration2.ProvideMidtermGradingCompleteFeature = !string.IsNullOrEmpty(webDefaults.StwebMidGradeCmplFlag) && (webDefaults.StwebMidGradeCmplFlag.ToUpper() == "Y");
                configuration2.LockMidtermGradingWhenComplete = !string.IsNullOrEmpty(webDefaults.StwebMidGradeLockFlag) && (webDefaults.StwebMidGradeLockFlag.ToUpper() == "Y");
                configuration2.FinalGradesLastDateAttendedDisplayBehavior = ConvertStringToLastDateAttendedNeverAttendedFieldDisplayType(webDefaults.StwebLdaFinalGrading);
                configuration2.MidtermGradesLastDateAttendedDisplayBehavior = ConvertStringToLastDateAttendedNeverAttendedFieldDisplayType(webDefaults.StwebLdaMidtermGrading);
                configuration2.FinalGradesNeverAttendedDisplayBehavior = ConvertStringToLastDateAttendedNeverAttendedFieldDisplayType(webDefaults.StwebNaFinalGrading);
                configuration2.MidtermGradesNeverAttendedDisplayBehavior = ConvertStringToLastDateAttendedNeverAttendedFieldDisplayType(webDefaults.StwebNaMidtermGrading);
                configuration2.RequireLastDateAttendedOrNeverAttendedFlagBeforeFacultyDrop = !string.IsNullOrEmpty(webDefaults.StwebRequireLdanaFacDrop) && (webDefaults.StwebRequireLdanaFacDrop.ToUpper() == "Y");
                configuration2.ShowPassAudit = !string.IsNullOrEmpty(webDefaults.StwebShowPassAudit) && (webDefaults.StwebShowPassAudit.ToUpper() == "Y");
                configuration2.ShowRepeated = !string.IsNullOrEmpty(webDefaults.StwebShowRepeated) && (webDefaults.StwebShowRepeated.ToUpper() == "Y");
            }

            if (webDefaults2 != null)
            {
                configuration2.IsGradingAllowedForDroppedWithdrawnStudents = (!string.IsNullOrEmpty(webDefaults2.Stweb2DisallowGrdeDrpWth) && webDefaults2.Stweb2DisallowGrdeDrpWth.ToUpper() == "Y") ? false : true;//On GRWP if the flag(DisallowGradingForDroppedWithdrawn) is Yes then it means faculty is not allowed to grade
                configuration2.IsGradingAllowedForNeverAttendedStudents = (!string.IsNullOrEmpty(webDefaults2.Stweb2DisallowGrdeNvrAtd) && webDefaults2.Stweb2DisallowGrdeNvrAtd.ToUpper() == "Y") ? false : true;//On GRWP if the flag(DisallowGradingForNeverAttended) is Yes then it means faculty is not allowed to grade
            }

            var allowedGradingTerms = await GetAllowedGradingTermsAsync();
            if (allowedGradingTerms != null && allowedGradingTerms.Any())
            {
                foreach (var term in allowedGradingTerms)
                {
                    try
                    {
                        configuration2.AddGradingTerm(term.Code);
                    }
                    catch (Exception ex)
                    {
                        // Ignore duplicates.
                        logger.Error(ex, "Duplicate grading term.");
                    }

                }
            }

            configuration2.VerifyGrades = !string.IsNullOrEmpty(webDefaults.StwebVerifyGrades) ? webDefaults.StwebVerifyGrades.Equals("Y", StringComparison.OrdinalIgnoreCase) ? true : false : default(bool?);
            // Catch and log exception if the either the STWEB.MIDTERM.GRADE.COUNT is not a number or if it isn't between 0 - 6. But continue either way and let it default to zero.
            if (!string.IsNullOrEmpty(webDefaults.StwebMidtermGradeCount))
            {
                try
                {
                    int midtermGradeCount;
                    if (int.TryParse(webDefaults.StwebMidtermGradeCount, out midtermGradeCount))
                    {
                        configuration2.NumberOfMidtermGrades = midtermGradeCount;
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
            return configuration2;
        }

        private async Task<IEnumerable<GradingTerm>> GetAllowedGradingTermsAsync()
        {
            return await GetValcodeAsync<GradingTerm>("ST", "GRADING.TERMS", r =>
                (new GradingTerm(r.ValInternalCodeAssocMember, r.ValExternalRepresentationAssocMember)), Level1CacheTimeoutValue);
        }

        /// <summary>
        /// Retrieves the configuration information needed for Colleague Self-Service instant enrollment
        /// </summary>
        public async Task<InstantEnrollmentConfiguration> GetInstantEnrollmentConfigurationAsync()
        {
            InstantEnrollmentConfiguration result;
            InstantEnrollmentConfiguration configuration = await GetOrAddToCacheAsync<InstantEnrollmentConfiguration>("InstantEnrollmentConfiguration",
                async () =>
                {
                    StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS");
                    StwebDefaults2 stwebDefaults2 = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults2>("ST.PARMS", "STWEB.DEFAULTS.2");
                    Ellucian.Colleague.Data.Student.DataContracts.CatalogSearchDefaults catalogSearchDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.CatalogSearchDefaults>("ST.PARMS", "CATALOG.SEARCH.DEFAULTS");
                    if (stwebDefaults == null)
                    {
                        var errorMessage = "Error while retrieving Colleague Self-Service instant enrollment configuration information; unable to retrieve ST.PARMS > STWEB.DEFAULTS.";
                        logger.Info(errorMessage);
                        throw new ApplicationException(errorMessage);
                    }
                    else
                    {
                        AddNewStudentProgramBehavior behavior = GetAddNewStudentProgramBehaviorFromStwebDefaults(stwebDefaults);
                        List<AcademicProgramOption> options = new List<AcademicProgramOption>();
                        if (stwebDefaults.WebCeAcadProgramsEntityAssociation != null)
                        {
                            foreach (var webCeAcadProgram in stwebDefaults.WebCeAcadProgramsEntityAssociation)
                            {
                                if (webCeAcadProgram != null)
                                {
                                    try
                                    {
                                        AcademicProgramOption option = new AcademicProgramOption(webCeAcadProgram.StwebCeAcadProgramsAssocMember, webCeAcadProgram.StwebCeCatalogsAssocMember);
                                        options.Add(option);
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.Info(ex, string.Format("Academic program option '{0}' is not a valid academic program option.", webCeAcadProgram.StwebCeAcadProgramsAssocMember));
                                    }
                                }
                            }
                        }

                        List<DemographicField> demographicFields = new List<DemographicField>();
                        if (stwebDefaults.StwebIePersonInfoFldsEntityAssociation != null)
                        {
                            foreach (var webIePersonInfoFld in stwebDefaults.StwebIePersonInfoFldsEntityAssociation)
                            {
                                if (webIePersonInfoFld != null)
                                {
                                    try
                                    {
                                        if (webIePersonInfoFld.StwebIePiFldCodeAssocMember.ToUpperInvariant() != "ADDRESS_LINES")
                                        {
                                            demographicFields.Add(new DemographicField(
                                                webIePersonInfoFld.StwebIePiFldCodeAssocMember,
                                                webIePersonInfoFld.StwebIePiFldDescAssocMember,
                                                ConvertStringToDemographicFieldRequirement(webIePersonInfoFld.StwebIePiFldStatusAssocMember)));
                                        }
                                        else
                                        {
                                            var requirement = ConvertStringToDemographicFieldRequirement(webIePersonInfoFld.StwebIePiFldStatusAssocMember);
                                            var additionalLinesRequirement = requirement == DemographicFieldRequirement.Required ? DemographicFieldRequirement.Optional : requirement;
                                            for (int i = 1; i < 5; i++)
                                            {
                                                demographicFields.Add(new DemographicField(
                                                    "ADDRESS_LINE_" + i.ToString(),
                                                    "Address Line " + i.ToString(),
                                                    i == 1 ? requirement : additionalLinesRequirement));
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.Info(ex, string.Format("Demographic field '{0}' is not a valid demographic field.", webIePersonInfoFld.StwebIePiFldCodeAssocMember));
                                    }
                                }
                            }
                        }

                        string paymentDistributionCode = !string.IsNullOrEmpty(stwebDefaults.StwebCeTenderGlDistCode) ? stwebDefaults.StwebCeTenderGlDistCode : stwebDefaults.StwebTenderGlDistrCode;
                        string citizenshipHomeCountryCode = stwebDefaults.StwebCitizenHomeCountry;
                        bool webPaymentsImplemented = !string.IsNullOrEmpty(stwebDefaults.StwebPayImplFlag) && stwebDefaults.StwebPayImplFlag.ToUpperInvariant() == "Y";
                        string registrationUserRole = stwebDefaults.StwebCeRegUserRole;
                        DateTime? searchEndDate = stwebDefaults.StwebCeEndDate;
                        bool showInstantEnrollmentBookstoreLink = !string.IsNullOrEmpty(stwebDefaults.StwebCeShowBkstrLnkFlag) && stwebDefaults.StwebCeShowBkstrLnkFlag.ToUpperInvariant() == "Y";
                        bool allowNonCitizenRegistration = false;

                        if (stwebDefaults2 == null)
                        {
                            var errorMessage = "Unable to access Instant Enrollment configuration information from ST.PARMS > STWEB.DEFAULTS.2.";
                            logger.Info(errorMessage);
                        }
                        else
                        {
                            allowNonCitizenRegistration = !string.IsNullOrEmpty(stwebDefaults2.Stweb2CeAllowNoncitznReg) && stwebDefaults2.Stweb2CeAllowNoncitznReg.ToUpperInvariant() == "Y";
                        }

                        result = new InstantEnrollmentConfiguration(behavior, options, paymentDistributionCode, citizenshipHomeCountryCode, webPaymentsImplemented,
                            registrationUserRole, searchEndDate, showInstantEnrollmentBookstoreLink, demographicFields, allowNonCitizenRegistration);
                        // Add CE Subjects from CECS into the configuration object
                        if (stwebDefaults.StwebCeSubjects != null && stwebDefaults.StwebCeSubjects.Any())
                        {
                            foreach (var subject in stwebDefaults.StwebCeSubjects)
                            {
                                result.AddSubjectCodeToDisplayInCatalog(subject);
                            }
                        }
                        // Add Instant Enrollment catalog search options from CATALOG.SEARCH.DEFAULTS
                        if (catalogSearchDefaults == null)
                        {
                            var errorMessage = "Unable to access Instant Enrollment catalog search defaults from ST.PARMS: CATALOG.SEARCH.DEFAULTS.";
                            logger.Info(errorMessage);
                        }
                        else
                        {
                            if (catalogSearchDefaults.ClsdCeSearchElementsEntityAssociation != null && catalogSearchDefaults.ClsdCeSearchElementsEntityAssociation.Any())
                            {
                                foreach (var filterOption in catalogSearchDefaults.ClsdCeSearchElementsEntityAssociation)
                                {
                                    if (filterOption != null && !String.IsNullOrEmpty(filterOption.ClsdCeSearchElementAssocMember))
                                    {
                                        switch (filterOption.ClsdCeSearchElementAssocMember.ToUpper())
                                        {
                                            case "AVAILABILITY":
                                                result.AddCatalogFilterOption(CatalogFilterType.Availability, filterOption.ClsdCeHideAssocMember.ToUpper() == "Y");
                                                break;
                                            case "LOCATIONS":
                                                result.AddCatalogFilterOption(CatalogFilterType.Locations, filterOption.ClsdCeHideAssocMember.ToUpper() == "Y");
                                                break;
                                            case "TERMS":
                                                result.AddCatalogFilterOption(CatalogFilterType.Terms, filterOption.ClsdCeHideAssocMember.ToUpper() == "Y");
                                                break;
                                            case "DAYS_OF_WEEK":
                                                result.AddCatalogFilterOption(CatalogFilterType.DaysOfWeek, filterOption.ClsdCeHideAssocMember.ToUpper() == "Y");
                                                break;
                                            case "TIMES_OF_DAY":
                                                result.AddCatalogFilterOption(CatalogFilterType.TimesOfDay, filterOption.ClsdCeHideAssocMember.ToUpper() == "Y");
                                                break;
                                            case "INSTRUCTORS":
                                                result.AddCatalogFilterOption(CatalogFilterType.Instructors, filterOption.ClsdCeHideAssocMember.ToUpper() == "Y");
                                                break;
                                            case "ACADEMIC_LEVELS":
                                                result.AddCatalogFilterOption(CatalogFilterType.AcademicLevels, filterOption.ClsdCeHideAssocMember.ToUpper() == "Y");
                                                break;
                                            case "COURSE_LEVELS":
                                                result.AddCatalogFilterOption(CatalogFilterType.CourseLevels, filterOption.ClsdCeHideAssocMember.ToUpper() == "Y");
                                                break;
                                            case "COURSE_TYPES":
                                                result.AddCatalogFilterOption(CatalogFilterType.CourseTypes, filterOption.ClsdCeHideAssocMember.ToUpper() == "Y");
                                                break;
                                            case "TOPICS":
                                                result.AddCatalogFilterOption(CatalogFilterType.TopicCodes, filterOption.ClsdCeHideAssocMember.ToUpper() == "Y");
                                                break;
                                            case "INSTRUCTION_TYPES":
                                                result.AddCatalogFilterOption(CatalogFilterType.InstructionTypes, filterOption.ClsdCeHideAssocMember.ToUpper() == "Y");
                                                break;
                                            case "SYNONYMS":
                                                result.AddCatalogFilterOption(CatalogFilterType.Synonyms, filterOption.ClsdCeHideAssocMember.ToUpper() == "Y");
                                                break;
                                            default:
                                                if (logger.IsInfoEnabled)
                                                {
                                                    logger.Info("Invalid entry in CATALOG.SEARCH.DEFAULTS " + filterOption.ClsdCeSearchElementAssocMember);
                                                }
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        return result;
                    }

                });
            return configuration;
        }

        private DemographicFieldRequirement ConvertStringToDemographicFieldRequirement(string demographicFieldRequirement)
        {
            if (string.IsNullOrEmpty(demographicFieldRequirement))
            {
                throw new ArgumentNullException("dfr", "A demographic field requirement must be specified.");
            }
            demographicFieldRequirement = demographicFieldRequirement.ToUpperInvariant();
            switch (demographicFieldRequirement)
            {
                case "R":
                    return DemographicFieldRequirement.Required;
                case "O":
                    return DemographicFieldRequirement.Optional;
                case "N":
                    return DemographicFieldRequirement.Hidden;
                default:
                    throw new ApplicationException("Demographic field requirement is not a valid value.");
            }
        }

        private AddNewStudentProgramBehavior GetAddNewStudentProgramBehaviorFromStwebDefaults(StwebDefaults stwebDefaults)
        {
            if (stwebDefaults.StwebCeAddStuPrograms == "ANY")
            {
                return AddNewStudentProgramBehavior.Any;
            }
            if (stwebDefaults.StwebCeAddStuPrograms == "NEW")
            {
                return AddNewStudentProgramBehavior.New;
            }
            throw new ApplicationException("STWEB.DEFAULTS > STWEB.CE.ADD.STU.PROGRAMS value '{0}' is not a valid value. Valid values are 'NEW' and 'ANY'.");
        }

        /// <summary>
        /// Get the unofficial transcript configurations
        /// </summary>
        /// <returns>The Unofficial Transcript Configuration entity</returns>
        public async Task<UnofficialTranscriptConfiguration> GetUnofficialTranscriptConfigurationAsync()
        {
            try
            {
                UnofficialTranscriptConfiguration result = new UnofficialTranscriptConfiguration();
                UnofficialTranscriptConfiguration configuration = await GetOrAddToCacheAsync<UnofficialTranscriptConfiguration>("UnofficialTranscriptConfiguration",
               async () =>
               {
                   StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS");

                   if (stwebDefaults == null)
                   {
                       var errorMessage = "Unable to access student web defaults from ST.PARMS. STWEB.DEFAULTS.";
                       logger.Info(errorMessage);
                       stwebDefaults = new StwebDefaults();
                   }
                   else
                   {
                       result.IsUseTanscriptFormat = !string.IsNullOrEmpty(stwebDefaults.StwebStUtUseFormatFlag) && stwebDefaults.StwebStUtUseFormatFlag.ToUpper() != "N";
                       result.FontSize = (stwebDefaults.StwebStUtReportFont.HasValue) ? stwebDefaults.StwebStUtReportFont.ToString() : "0";
                       result.PageHeight = (stwebDefaults.StwebStUtReportHeight.HasValue) ? stwebDefaults.StwebStUtReportHeight.ToString() : "0";
                       result.PageWidth = (stwebDefaults.StwebStUtReportWidth.HasValue) ? stwebDefaults.StwebStUtReportWidth.ToString() : "0";
                       result.TopMargin = (stwebDefaults.StwebStUtReportTMargin.HasValue) ? stwebDefaults.StwebStUtReportTMargin.ToString() : "0";
                       result.RightMargin = (stwebDefaults.StwebStUtReportRMargin.HasValue) ? stwebDefaults.StwebStUtReportRMargin.ToString() : "0";
                       result.BottomMargin = (stwebDefaults.StwebStUtReportBMargin.HasValue) ? stwebDefaults.StwebStUtReportBMargin.ToString() : "0";
                       result.LeftMargin = (stwebDefaults.StwebStUtReportLMargin.HasValue) ? stwebDefaults.StwebStUtReportLMargin.ToString() : "0";
                   }
                   return result;
               });
                return configuration;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while getting the unofficial transcript configurations");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Get My Progress configuration information
        /// </summary>
        /// <returns>The MyProgressConfiguration entity</returns>
        public async Task<MyProgressConfiguration> GetMyProgressConfigurationAsync()
        {
            MyProgressConfiguration myProgressConfiguration = await GetOrAddToCacheAsync<MyProgressConfiguration>("MyProgressConfiguration",
               async () =>
               {
                   Data.Student.DataContracts.DaDefaults daDefaults = await DataReader.ReadRecordAsync<Data.Student.DataContracts.DaDefaults>("ST.PARMS", "DA.DEFAULTS");
                   Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults2 stwebDefaults2 = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults2>("ST.PARMS", "STWEB.DEFAULTS.2");

                   if (daDefaults == null)
                   {
                       var errorMessage = "Unable to access DA.DEFAULTS from ST.PARMS table.";
                       logger.Info(errorMessage);
                       throw new ColleagueWebApiException(errorMessage);
                   }
                   if (stwebDefaults2 == null)
                   {
                       var errorMessage = "Unable to access student web defaults from ST.PARMS. STWEB.DEFAULTS.2";
                       logger.Info(errorMessage);
                       stwebDefaults2 = new StwebDefaults2();
                   }
                   bool showAcadLevelStandingFlag = (!string.IsNullOrEmpty(daDefaults.DaAcadLevelStandingFlag) && daDefaults.DaAcadLevelStandingFlag.ToUpper() == "Y") ? true : false;
                   bool hideProgressBarOverallProgressFlag = (!string.IsNullOrEmpty(stwebDefaults2.Stweb2HidePrgsBrTotPrgs) && stwebDefaults2.Stweb2HidePrgsBrTotPrgs.ToUpper() == "Y") ? true : false;
                   bool hideProgressBarTotalCreditsFlag = (!string.IsNullOrEmpty(stwebDefaults2.Stweb2HidePrgsBrTotCred) && stwebDefaults2.Stweb2HidePrgsBrTotCred.ToUpper() == "Y") ? true : false;
                   bool hideProgressBarInstitutionalCreditsFlag = (!string.IsNullOrEmpty(stwebDefaults2.Stweb2HidePrgsBrInCred) && stwebDefaults2.Stweb2HidePrgsBrInCred.ToUpper() == "Y") ? true : false;
                   bool showPseudoCoursesInRequirements = (!string.IsNullOrEmpty(daDefaults.DaShowPseudoCrsesInReqs) && daDefaults.DaShowPseudoCrsesInReqs.ToUpper() == "Y") ? true : false;

                   MyProgressConfiguration config = new MyProgressConfiguration(showAcadLevelStandingFlag,
                       hideProgressBarOverallProgressFlag,
                       hideProgressBarTotalCreditsFlag,
                       hideProgressBarInstitutionalCreditsFlag,
                       showPseudoCoursesInRequirements);

                   return config;
               });

            return myProgressConfiguration;
        }

        /// <summary>
        /// Retrieves the section census configuration information needed for Colleague Self-Service
        /// </summary>
        public async Task<SectionCensusConfiguration> GetSectionCensusConfigurationAsync()
        {
            SectionCensusConfiguration sectionCensusConfiguration = await GetOrAddToCacheAsync<SectionCensusConfiguration>("SectionCensusConfiguration",
                async () =>
                {
                    SectionCensusConfiguration configuration;
                    StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS");
                    if (stwebDefaults == null)
                    {
                        var errorMessage = "Unable to access student web defaults from ST.PARMS. STWEB.DEFAULTS.";
                        logger.Info(errorMessage);
                        throw new ApplicationException(errorMessage);
                    }

                    LastDateAttendedNeverAttendedFieldDisplayType ldaNaCensusRoster = ConvertStringToLastDateAttendedNeverAttendedFieldDisplayType(stwebDefaults.StwebLdaNaCensusRoster);

                    var censusDatePositionSubmissions = new List<CensusDatePositionSubmission>();
                    if (stwebDefaults.CensusDatePositionsEntityAssociation != null)
                    {
                        foreach (var censusDateSubmission in stwebDefaults.CensusDatePositionsEntityAssociation)
                        {
                            if (censusDateSubmission != null)
                            {
                                try
                                {
                                    CensusDatePositionSubmission censusDatePositionSubmission = new CensusDatePositionSubmission(
                                        position: censusDateSubmission.StwebCensusDatePositionsAssocMember,
                                        label: censusDateSubmission.StwebCensusDateLabelsAssocMember,
                                        certifyDaysBeforeOffset: censusDateSubmission.StwebCensusDateDaysPriorAssocMember);

                                    censusDatePositionSubmissions.Add(censusDatePositionSubmission);
                                }
                                catch (Exception ex)
                                {
                                    logger.Info(ex, string.Format("Census date position submission '{0}' is not a valid census date position submission.", censusDateSubmission.StwebCensusDatePositionsAssocMember));
                                }
                            }
                        }
                    }

                    configuration = new SectionCensusConfiguration(lastDateAttendedNeverAttendedCensusRoster: ldaNaCensusRoster,
                        censusDatePositionSubmissions: censusDatePositionSubmissions, facultyDropReasonCode: stwebDefaults.StwebDfltFctyDropReason);
                    return configuration;
                });
            return sectionCensusConfiguration;
        }

        /// <summary>
        /// Retrieves the section census configuration information needed for Colleague Self-Service
        /// </summary>
        public async Task<SectionCensusConfiguration2> GetSectionCensusConfiguration2Async()
        {
            SectionCensusConfiguration2 sectionCensusConfiguration2 = await GetOrAddToCacheAsync<SectionCensusConfiguration2>("SectionCensusConfiguration2",
                async () =>
                {
                    SectionCensusConfiguration2 configuration2;
                    StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS");
                    if (stwebDefaults == null)
                    {
                        var errorMessage = "Unable to access student web defaults from ST.PARMS. STWEB.DEFAULTS.";
                        logger.Info(errorMessage);
                        throw new ApplicationException(errorMessage);
                    }

                    LastDateAttendedNeverAttendedFieldDisplayType lastDateAttendedCensusRoster =
                        ConvertStringToLastDateAttendedNeverAttendedFieldDisplayType(stwebDefaults.StwebLdaCensusRoster);
                    LastDateAttendedNeverAttendedFieldDisplayType neverAttendedCensusRoster =
                        ConvertStringToLastDateAttendedNeverAttendedFieldDisplayType(stwebDefaults.StwebNaCensusRoster);

                    var censusDatePositionSubmissions = new List<CensusDatePositionSubmission>();
                    if (stwebDefaults.CensusDatePositionsEntityAssociation != null)
                    {
                        foreach (var censusDateSubmission in stwebDefaults.CensusDatePositionsEntityAssociation)
                        {
                            if (censusDateSubmission != null)
                            {
                                try
                                {
                                    CensusDatePositionSubmission censusDatePositionSubmission = new CensusDatePositionSubmission(
                                        position: censusDateSubmission.StwebCensusDatePositionsAssocMember,
                                        label: censusDateSubmission.StwebCensusDateLabelsAssocMember,
                                        certifyDaysBeforeOffset: censusDateSubmission.StwebCensusDateDaysPriorAssocMember);

                                    censusDatePositionSubmissions.Add(censusDatePositionSubmission);
                                }
                                catch (Exception ex)
                                {
                                    logger.Info(ex, string.Format("Census date position submission '{0}' is not a valid census date position submission.", censusDateSubmission.StwebCensusDatePositionsAssocMember));
                                }
                            }
                        }
                    }

                    configuration2 = new SectionCensusConfiguration2(lastDateAttendedCensusRoster: lastDateAttendedCensusRoster,
                        neverAttendedCensusRoster: neverAttendedCensusRoster,
                        censusDatePositionSubmissions: censusDatePositionSubmissions, facultyDropReasonCode: stwebDefaults.StwebDfltFctyDropReason);
                    return configuration2;
                });
            return sectionCensusConfiguration2;
        }

        private LastDateAttendedNeverAttendedFieldDisplayType ConvertStringToLastDateAttendedNeverAttendedFieldDisplayType(string ldaNaFieldDisplayType)
        {
            //default to editable when not set
            if (string.IsNullOrWhiteSpace(ldaNaFieldDisplayType))
            {
                return LastDateAttendedNeverAttendedFieldDisplayType.Editable;
            }

            ldaNaFieldDisplayType = ldaNaFieldDisplayType.ToUpperInvariant();

            switch (ldaNaFieldDisplayType)
            {
                case "E":
                    return LastDateAttendedNeverAttendedFieldDisplayType.Editable;
                case "R":
                    return LastDateAttendedNeverAttendedFieldDisplayType.ReadOnly;
                case "H":
                    return LastDateAttendedNeverAttendedFieldDisplayType.Hidden;
                default:
                    {
                        var errorMessage = string.Format("The LDA/NA field display type {0} is not a valid value.", ldaNaFieldDisplayType);
                        logger.Info(errorMessage);
                        throw new ApplicationException(errorMessage);
                    }
            }
        }

        /// <summary>
        /// Get Academic Record configuration information
        /// </summary>
        /// <returns>The AcademicRecordConfiguration entity</returns>
        public async Task<AcademicRecordConfiguration> GetAcademicRecordConfigurationAsync()
        {
            AcademicRecordConfiguration academicRecordConfiguration = await GetOrAddToCacheAsync<AcademicRecordConfiguration>("AcademicRecordConfiguration",
               async () =>
               {
                   Data.Student.DataContracts.AcDefaults acDefaults = await DataReader.ReadRecordAsync<Data.Student.DataContracts.AcDefaults>("ST.PARMS", "AC.DEFAULTS");

                   if (acDefaults == null)
                   {
                       var errorMessage = "Unable to access AC.DEFAULTS from ST.PARMS table.";
                       logger.Info(errorMessage);
                       throw new ColleagueWebApiException(errorMessage);
                   }

                   AnonymousGradingType anonymousGradingType = AnonymousGradingType.None;

                   switch (acDefaults.AcdRandomIdAssign.ToUpperInvariant())
                   {
                       case "S":
                           anonymousGradingType = AnonymousGradingType.Section;
                           break;
                       case "T":
                           anonymousGradingType = AnonymousGradingType.Term;
                           break;
                   }

                   var config = new AcademicRecordConfiguration(anonymousGradingType);
                   return config;
               });

            return academicRecordConfiguration;
        }

        /// <summary>
        /// Get the faculty attendance configuration information
        /// </summary>
        /// <returns>The FacultyAttendanceConfiguration entity</returns>
        public async Task<FacultyAttendanceConfiguration> GetFacultyAttendanceConfigurationAsync()
        {
            try
            {
                FacultyAttendanceConfiguration facultyAttendanceConfiguration = await GetOrAddToCacheAsync<FacultyAttendanceConfiguration>("FacultyAttendanceConfiguration",
                    async () =>
                    {
                        var configuration = new FacultyAttendanceConfiguration();

                        var stwebDefaults2 = await DataReader.ReadRecordAsync<StwebDefaults2>("ST.PARMS", "STWEB.DEFAULTS.2");
                        if (stwebDefaults2 == null)
                        {
                            var errorMessage = "Unable to access faculty attendance web defaults from ST.PARMS STWEB.DEFAULTS.2";
                            logger.Info(errorMessage);
                            throw new ColleagueWebApiException(errorMessage);
                        }
                        configuration.CloseAttendanceCensusTrackNumber = stwebDefaults2.Stweb2ClsAttCensusNum;
                        configuration.CloseAttendanceNumberOfDaysPastCensusTrackDate = stwebDefaults2.Stweb2ClsAttDaysPastCen;
                        configuration.CloseAttendanceNumberOfDaysPastSectionEndDate = stwebDefaults2.Stweb2ClsAttDaysPastSec;
                        return configuration;

                    });
                return facultyAttendanceConfiguration;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while retrieving faculty attendance configuration information.");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An exception occurred while retrieving faculty attendance configuration information.");
                throw;
            }
        }

        /// <summary>
        /// Get the student records release configuration information
        /// </summary>
        /// <returns>The StudentRecordsReleaseConfig entity</returns>
        public async Task<StudentRecordsReleaseConfig> GetStudentRecordsReleaseConfigAsync()
        {
            try
            {
                var studentRecordReleaseConfiguration = new StudentRecordsReleaseConfig();

                var stwebDefaults2 = await DataReader.ReadRecordAsync<StwebDefaults2>("ST.PARMS", "STWEB.DEFAULTS.2", false);               
                if (stwebDefaults2 == null)
                {
                    var errorMessage = "Unable to access student records release configuration information from ST.PARMS STWEB.DEFAULTS.2";
                    logger.Info(errorMessage);
                    throw new ColleagueWebApiException(errorMessage);
                }
                //replace two _vms with a new line and replace remaining single vms with a space. 
                if (!string.IsNullOrEmpty(stwebDefaults2.Stweb2StRecRelText))
                {
                    string[] vmString = { _VM.ToString() + " " + _VM.ToString(), _VM.ToString() + _VM.ToString() };
                    studentRecordReleaseConfiguration.Text = stwebDefaults2.Stweb2StRecRelText.Split(vmString, StringSplitOptions.None).Select(a => a.Replace(_VM, ' ')).ToList();
                }
                studentRecordReleaseConfiguration.IsPinRequired = (!string.IsNullOrEmpty(stwebDefaults2.Stweb2StRecRelPinReq) && stwebDefaults2.Stweb2StRecRelPinReq.ToUpper() == "Y") ? true : false;

                return studentRecordReleaseConfiguration;

            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while retrieving student records release configuration information.");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An exception occurred while retrieving student records release configuration information.");
                throw;
            }
        }
    }
}
