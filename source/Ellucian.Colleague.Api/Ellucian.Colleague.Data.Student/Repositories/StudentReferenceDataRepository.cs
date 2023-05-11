// Copyright 2016-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Repository for student reference data
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentReferenceDataRepository : BaseApiRepository, IStudentReferenceDataRepository
    {
        private Data.Base.DataContracts.IntlParams internationalParameters;

        // Sets the maximum number of records to bulk read at one time
        readonly int readSize;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cacheProvider">Cache provider interface</param>
        /// <param name="transactionFactory">Transaction factory interface</param>
        /// <param name="logger">Logging interface</param>
        /// <param name="apiSettings">API settings</param>
        public StudentReferenceDataRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;

            // Use the bulk read size from API settings, or fall back to 5000
            this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }

        public async Task<IEnumerable<AcademicDepartment>> GetAcademicDepartmentsAsync()
        {
            return await GetAcademicDepartmentsAsync(false);
        }

        /// <summary>
        /// Get a collection of academic departments
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of academic departments</returns>
        public async Task<IEnumerable<AcademicDepartment>> GetAcademicDepartmentsAsync(bool ignoreCache = false)
        {
            if (ignoreCache)
            {
                var academicDepartments = await BuildAllAcademicDepartments();
                return await AddOrUpdateCacheAsync<IEnumerable<AcademicDepartment>>("AllOfferingDepartments", academicDepartments);

            }
            else
            {
                return await GetOrAddToCacheAsync<IEnumerable<AcademicDepartment>>("AllOfferingDepartments", async () => await this.BuildAllAcademicDepartments(), Level1CacheTimeoutValue);
            }
        }


        private async Task<IEnumerable<AcademicDepartment>> BuildAllAcademicDepartments()
        {
            var academicDepartmentEntities = new List<AcademicDepartment>();
            
            var academicDepartmentRecords = await DataReader.BulkReadRecordAsync<Depts>("DEPTS", "");
            
 
            foreach (var academicDepartmentRecord in academicDepartmentRecords)
            {
               
                var ad = new AcademicDepartment(academicDepartmentRecord.RecordGuid, 
                    academicDepartmentRecord.Recordkey, academicDepartmentRecord.DeptsDesc, "A".Equals(academicDepartmentRecord.DeptsActiveFlag))
                {
                    AcademicLevelCode = academicDepartmentRecord.DeptsAcadLevel, 
                    GradeSchemeCode = academicDepartmentRecord.DeptsGradeScheme
                };
                academicDepartmentEntities.Add(ad);

            }

            return academicDepartmentEntities;
        }

        public async Task<IEnumerable<AcademicLevel>> GetAcademicLevelsAsync()
        {
            return await GetAcademicLevelsAsync(false);
        }

        /// <summary>
        /// Get a collection of academic levels
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of academic levels</returns>
        public async Task<IEnumerable<AcademicLevel>> GetAcademicLevelsAsync(bool ignoreCache = false)
        {
            return await GetGuidCodeItemAsync<AcadLevels, AcademicLevel>("AllAcademicLevels", "ACAD.LEVELS",
                (al, g) => new AcademicLevel(g, al.Recordkey, al.AclvDesc)
                {
                    GradeScheme = al.AclvGradeScheme,
                    Category = !string.IsNullOrEmpty(al.AclvCeLevelFlag) && al.AclvCeLevelFlag.ToUpper().Equals("Y") ? true : false
                }, 
                bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for AcademicLevels code
        /// </summary>
        /// <param name="code">AcademicLevels code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetAcademicLevelsGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetAcademicLevelsAsync(false);
            AcademicLevel codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }
            
            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAcademicLevelsAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ACAD.LEVELS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ACAD.LEVELS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ACAD.LEVELS', Record ID:'", code, "'"));
            }
            return guid;

        }

        /// <summary>
        /// Get a collection of academic programs
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of academic programs</returns>
        public async Task<IEnumerable<AcademicProgram>> GetAcademicProgramsAsync(bool ignoreCache = false)
        {
            return await GetGuidCodeItemAsync<AcadPrograms, AcademicProgram>("AllAcademicPrograms", "ACAD.PROGRAMS",
                (ap, g) => BuildAcademicProgram(ap, g), bypassCache: ignoreCache);
        }


        /// <summary>
        /// Get guid for AcademicPrograms code
        /// </summary>
        /// <param name="code">AcademicPrograms code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetAcademicProgramsGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetAcademicProgramsAsync(false);
            AcademicProgram codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }
            
            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAcademicProgramsAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ACAD.PROGRAMS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ACAD.PROGRAMS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ACAD.PROGRAMS', Record ID:'", code, "'"));
            }
            return guid;

        }

        /// <summary>
        /// Get an academic program
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>academic programs</returns>
        public async Task<AcademicProgram> GetAcademicProgramByGuidAsync(string guid)
        {
            GuidLookupResult lookupResult = await GetGuidLookupResultFromGuidAsync(guid);

            if (lookupResult.Entity != "ACAD.PROGRAMS")
            {
                var ex = new RepositoryException();
                var msg = "GUID " + guid + " has different entity, " + lookupResult.Entity + ", than expected, ACAD.PROGRAMS";
                ex.AddError(new Domain.Entities.RepositoryError("GUID.Wrong.Type", msg));
                throw ex;
            }
            AcadPrograms acadProgram = await DataReader.ReadRecordAsync<AcadPrograms>(lookupResult.PrimaryKey, true);
            return BuildAcademicProgram(acadProgram, guid);
        }

        /// <summary>
        /// Builds an Academic Program
        /// </summary>
        /// <param name="ap">AcadPrograms record</param>
        /// <param name="g">guid</param>
        /// <returns>Academic Program</returns>
        private AcademicProgram BuildAcademicProgram(AcadPrograms ap, string g)
        {
            var prog = new AcademicProgram(g, ap.Recordkey, ap.AcpgTitle)
            {
                LongDescription = (!string.IsNullOrEmpty(ap.AcpgDesc) ? ap.AcpgDesc.Replace(DmiString.sVM, string.Empty) : null),
                DegreeCode = ap.AcpgDegree,
                MajorCodes = ap.AcpgMajors,
                MinorCodes = ap.AcpgMinors,
                CertificateCodes = ap.AcpgCcds,
                SpecializationCodes = ap.AcpgSpecializations,
                HonorCode = ap.AcpgHonorsCode,
                AcadLevelCode = ap.AcpgAcadLevel,
                StartDate = ap.AcpgStartDate,
                EndDate = ap.AcpgEndDate,
                FederalCourseClassification = ap.AcpgCip,
                LocalCourseClassifications = ap.AcpgLocalGovtCodes,
                DeptartmentCodes = ap.AcpgDepts,
                Location = ap.AcpgLocations,
                AuthorizingInstitute = ap.AcpgApprovalAgencyIds,
                AddnlCcds = ap.AcpgAddnlCcds,
                AddnlMajors = ap.AcpgAddnlMajors,
                AddnlMinors = ap.AcpgAddnlMinors,
                AddnlSpecializations = ap.AcpgAddnlSpecializations
            };
            return prog;
        }




        public async Task<IEnumerable<AcademicStanding>> GetAcademicStandingsAsync()
        {
            var academicStandings = await GetValcodeAsync<AcademicStanding>("ST", "ACAD.STANDINGS",
                at =>
                {
                    try
                    {
                        return new AcademicStanding(at.ValInternalCodeAssocMember, at.ValExternalRepresentationAssocMember);
                    }
                    catch (Exception e)
                    {
                        // Log and return null for codes without a description.
                        LogDataError("ACAD.STANDINGS", at.ValInternalCodeAssocMember, null, e, string.Format("Failed to add academic standing {0}", at.ValInternalCodeAssocMember));
                        return null;
                    }
                }
                );
            // Exclude nulls from codes without a description.
            return academicStandings.Where(ast => ast != null).ToList();
        }

        /// <summary>
        /// Get a collection of academic standings
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of academic standings</returns>
        public async Task<IEnumerable<AcademicStanding2>> GetAcademicStandings2Async(bool ignoreCache = false)
        {
            return await GetGuidValcodeAsync<AcademicStanding2>("ST", "ACAD.STANDINGS",
                (a, g) => new AcademicStanding2(g, a.ValInternalCodeAssocMember, (string.IsNullOrEmpty(a.ValExternalRepresentationAssocMember)
                    ? a.ValInternalCodeAssocMember : a.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of ArCategories
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of ArCategories</returns>
        public async Task<IEnumerable<ArCategory>> GetArCategoriesAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<ArCategories, ArCategory>("AllEEDMArCategories", "AR.CATEGORIES",
                (ac, g) => new ArCategory(g, ac.Recordkey, ac.ArctDesc), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Gets AccountingCodes
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<AccountingCode>> GetAccountingCodesAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<ArCodes, AccountingCode>("AllEEDMAccountingCodes", "AR.CODES",
            (ac, g) => new AccountingCode(g, ac.Recordkey, ac.ArcDesc) { ArCategoryCode = ac.ArcArCategory }, bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get All CIP codes for cip-codes API (HEDM)
        /// </summary>
        /// <returns>List of CipCode entities</returns>
        public async Task<IEnumerable<CipCode>> GetCipCodesAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<Cip, CipCode>("AllHedmCipCodes", "CIP",
                (ac, g) => new CipCode(g, ac.Recordkey, !string.IsNullOrEmpty(ac.CipDesc) ? ac.CipDesc : ac.Recordkey, ac.CipRevisionYear), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for Cip code
        /// </summary>
        /// <param name="code">Cip code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetCipCodesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetCipCodesAsync(false);
            CipCode codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetCipCodesAsync(true);
                if (allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CIP', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CIP', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CIP', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// Get code for Cip code guid
        /// </summary>
        /// <param name="code">Cip code guid</param>
        /// <returns>code</returns>
        public async Task<string> GetCipCodesFromGuidAsync(string guid)
        {
            //get all the codes from the cache
            string code = string.Empty;
            if (string.IsNullOrEmpty(guid))
                return code;
            var allCodesCache = await GetCipCodesAsync(false);
            CipCode codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetCipCodesAsync(true);
                if (allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No code found, Entity:'CIP', Record ID:'", guid, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    code = codeNoCache.Code;
                else
                    throw new RepositoryException(string.Concat("No code found, Entity:'CIP', Record ID:'", guid, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Code))
                    code = codeCache.Code;
                else
                    throw new RepositoryException(string.Concat("No code found, Entity:'CIP', Record ID:'", guid, "'"));
            }
            return code;
        }

        /// <summary>
        /// Gets DistributionMethod
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<DistributionMethod>> GetDistrMethodCodesAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<ArCodes, DistributionMethod>("AllDistrMethods", "DISTRIBUTION",
            (ac, g) => new DistributionMethod(g, ac.Recordkey, ac.ArcDesc), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for DistributionMethod code
        /// </summary>
        /// <param name="code">Distribution code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetDistrMethodGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetDistrMethodCodesAsync(false);
            DistributionMethod codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetDistrMethodCodesAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'DISTRIBUTION', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'DISTRIBUTION', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'DISTRIBUTION', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// Get code for DistributionMethod guid
        /// </summary>
        /// <param name="code">DistributionMethod guid</param>
        /// <returns>code</returns>
        public async Task<string> GetDistrMethodCodeFromGuidAsync(string guid)
        {
            //get all the codes from the cache
            string code = string.Empty;
            if (string.IsNullOrEmpty(guid))
                return code;
            var allCodesCache = await GetDistrMethodCodesAsync(false);
            DistributionMethod codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetDistrMethodCodesAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No code found, Entity:'DISTRIBUTION', Record ID:'", guid, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    code = codeNoCache.Code;
                else
                    throw new RepositoryException(string.Concat("No code found, Entity:'DISTRIBUTION', Record ID:'", guid, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Code))
                    code = codeCache.Code;
                else
                    throw new RepositoryException(string.Concat("No code found, Entity:'DISTRIBUTION', Record ID:'", guid, "'"));
            }
            return code;
        }

        /// <summary>
        /// Gets AccountReceivableTypes
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<AccountReceivableType>> GetAccountReceivableTypesAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<ArTypes, AccountReceivableType>("AllEEDMAccountReceivableTypes", "AR.TYPES",
            (ar, g) => new AccountReceivableType(g, ar.Recordkey, ar.ArtDesc), bypassCache: ignoreCache);
        }


        /// <summary>
        /// Get guid for AccountReceivableTypes code
        /// </summary>
        /// <param name="code">AccountReceivableTypes code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetAccountReceivableTypesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetAccountReceivableTypesAsync(false);
            AccountReceivableType codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAccountReceivableTypesAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'AR.TYPES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'AR.TYPES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'AR.TYPES', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// Get code for AccountReceivableTypes guid
        /// </summary>
        /// <param name="code">AccountReceivableTypes guid</param>
        /// <returns>code</returns>
        public async Task<string> GetAccountReceivableTypesCodeFromGuidAsync(string guid)
        {
            //get all the codes from the cache
            string code = string.Empty;
            if (string.IsNullOrEmpty(guid))
                return code;
            var allCodesCache = await GetAccountReceivableTypesAsync(false);
            AccountReceivableType codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAccountReceivableTypesAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No code found, Entity:'AR.TYPES', Record ID:'", guid, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    code = codeNoCache.Code;
                else
                    throw new RepositoryException(string.Concat("No code found, Entity:'AR.TYPES', Record ID:'", guid, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Code))
                    code = codeCache.Code;
                else
                    throw new RepositoryException(string.Concat("No code found, Entity:'AR.TYPES', Record ID:'", guid, "'"));
            }
            return code;
        }

        /// <summary>
        /// Gets AccountReceivableDepositTypes
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<AccountReceivableDepositType>> GetAccountReceivableDepositTypesAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<ArDepositTypes, AccountReceivableDepositType>("AllEEDMAccountReceivableDepositTypes", "AR.DEPOSIT.TYPES",
            (ac, g) => new AccountReceivableDepositType(g, ac.Recordkey, ac.ArdtDesc), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Gets Distribution2
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Distribution2>> GetDistributionsAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<Distributions, Distribution2>("AllEEDMDistributions2", "DISTRIBUTION",
            (ac, g) => new Distribution2(g, ac.Recordkey, ac.DistrDescription), bypassCache: ignoreCache);
        }

        public async Task<IEnumerable<AdmissionApplicationType>> GetAdmissionApplicationTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<AdmissionApplicationType>("ST", "INTG.APPLICATION.TYPES",
                (cl, g) => new AdmissionApplicationType(g, cl.ValInternalCodeAssocMember, cl.ValExternalRepresentationAssocMember), bypassCache: ignoreCache);

        }

        /// <summary>
        /// Get guid for AdmissionApplicationTypes code
        /// </summary>
        /// <param name="code">AdmissionApplicationTypes code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetAdmissionApplicationTypesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetAdmissionApplicationTypesAsync(false);
            AdmissionApplicationType codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAdmissionApplicationTypesAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'INTG.APPLICATION.TYPES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'INTG.APPLICATION.TYPES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'INTG.APPLICATION.TYPES', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// Get guid for AdmissionApplicationStatusTypes code
        /// </summary>
        /// <param name="code">AdmissionApplicationStatusTypes code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetAdmissionApplicationStatusTypesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetAdmissionApplicationStatusTypesAsync(false);
            AdmissionApplicationStatusType codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAdmissionApplicationStatusTypesAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'APPLICATION.STATUSES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'APPLICATION.STATUSES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'APPLICATION.STATUSES', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// Gets & caches application statuses EEDM v6
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<AdmissionApplicationStatusType>> GetAdmissionApplicationStatusTypesAsync(bool bypassCache)
        {
            if (bypassCache)
            {
                return await BuildAllAdmissionApplicationStatusTypes();
            }
            else
            {
                return await GetOrAddToCacheAsync<IEnumerable<AdmissionApplicationStatusType>>("AllAdmissionApplicationStatusTypes", async () => await this.BuildAllAdmissionApplicationStatusTypes(), Level1CacheTimeoutValue);
            }
        }

        private async Task<IEnumerable<AdmissionApplicationStatusType>> BuildAllAdmissionApplicationStatusTypes()
        { 
            var admissionApplicationStatusEntities = new List<AdmissionApplicationStatusType>();
            var admissionApplicationStatusIds = await DataReader.SelectAsync("APPLICATION.STATUSES", null);

            var admissionApplicationStatusRecords = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.ApplicationStatuses>(admissionApplicationStatusIds);

            foreach (var admissionApplicationStatusRecord in admissionApplicationStatusRecords)
            {
                var admissionApplicationStatusGuidInfo = await GetGuidFromRecordInfoAsync("APPLICATION.STATUSES", admissionApplicationStatusRecord.Recordkey, null, null);
                admissionApplicationStatusEntities.Add(new AdmissionApplicationStatusType(admissionApplicationStatusGuidInfo, admissionApplicationStatusRecord.Recordkey, !string.IsNullOrEmpty(admissionApplicationStatusRecord.AppsDesc) ? admissionApplicationStatusRecord.AppsDesc : admissionApplicationStatusRecord.Recordkey)
                { AdmissionApplicationStatusTypesCategory = GetAdmissionApplicationType(admissionApplicationStatusRecord.AppsSpecialProcessingCode), SpecialProcessingCode = admissionApplicationStatusRecord.AppsSpecialProcessingCode });
            }
            return admissionApplicationStatusEntities;
        }

        /// <summary>
        /// Get guid for AdmissionPopulations code
        /// </summary>
        /// <param name="code">AdmissionPopulations code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetAdmissionPopulationsGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetAdmissionPopulationsAsync(false);
            AdmissionPopulation codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAdmissionPopulationsAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ADMIT.STATUSES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ADMIT.STATUSES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ADMIT.STATUSES', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// Get a collection of AdmissionPopulations
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of AdmissionPopulations</returns>
        public async Task<IEnumerable<AdmissionPopulation>> GetAdmissionPopulationsAsync(bool ignoreCache)
        {

            if (ignoreCache)
            {
                return await BuildAllAdmissionPopulations();
            }
            else
            {
                return await GetOrAddToCacheAsync<IEnumerable<AdmissionPopulation>>("AllAdmissionPopulations", async () => await this.BuildAllAdmissionPopulations(), Level1CacheTimeoutValue);
            }
        }
        
        private async Task<IEnumerable<AdmissionPopulation>> BuildAllAdmissionPopulations()
        {
            var admissionPopulationsEntities = new List<AdmissionPopulation>();
            var admissionPopulationsRecords = await DataReader.BulkReadRecordAsync<DataContracts.AdmitStatuses>("ADMIT.STATUSES", "");
            foreach (var admissionPopulationsRecord in admissionPopulationsRecords)
            {
                if ((!string.IsNullOrEmpty(admissionPopulationsRecord.RecordGuid)) && (!string.IsNullOrEmpty(admissionPopulationsRecord.Recordkey)) && (!string.IsNullOrEmpty(admissionPopulationsRecord.AdmsDesc)))
                {
                    var admissionPopulation = new AdmissionPopulation(admissionPopulationsRecord.RecordGuid, admissionPopulationsRecord.Recordkey, admissionPopulationsRecord.AdmsDesc);
                    admissionPopulationsEntities.Add(admissionPopulation);
                }
                else
                {
                    if ((!string.IsNullOrEmpty(admissionPopulationsRecord.Recordkey)) && (string.IsNullOrEmpty(admissionPopulationsRecord.RecordGuid)))
                    {
                        throw new RepositoryException(string.Concat("Missing GUID for ADMIT.STATUSES record ID:'", admissionPopulationsRecord.Recordkey, "'"));
                    }
                }
            }
            return admissionPopulationsEntities;
        }

        /// <summary>
        /// Get a collection of admission residency types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of admission residency types</returns>
        public async Task<IEnumerable<AdmissionResidencyType>> GetAdmissionResidencyTypesAsync(bool ignoreCache = false)
        {
            return await GetGuidCodeItemAsync<ResidencyStatuses, AdmissionResidencyType>("AllAdmissionResidencyTypes", "RESIDENCY.STATUSES",
                (ar, g) => new AdmissionResidencyType(g, ar.Recordkey, !string.IsNullOrEmpty(ar.ResDesc) ? ar.ResDesc : ar.Recordkey), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for AdmissionResidencyTypes code
        /// </summary>
        /// <param name="code">AdmissionResidencyTypes code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetAdmissionResidencyTypesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetAdmissionResidencyTypesAsync(false);
            AdmissionResidencyType codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAdmissionResidencyTypesAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'RESIDENCY.STATUSES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'RESIDENCY.STATUSES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'RESIDENCY.STATUSES', Record ID:'", code, "'"));
            }
            return guid;
        }

        public async Task<IEnumerable<AdvisorType>> GetAdvisorTypesAsync(bool ignoreCache = false)
        {
            var advisorTypes = await GetGuidValcodeAsync<AdvisorType>("ST", "ADVISOR.TYPES",
                (at, g) =>
                {
                    try
                    {
                        return new AdvisorType(g, at.ValInternalCodeAssocMember, 
                            string.IsNullOrWhiteSpace(at.ValExternalRepresentationAssocMember) ? at.ValInternalCodeAssocMember : at.ValExternalRepresentationAssocMember, 
                            at.ValActionCode1AssocMember);
                    }
                    catch (ColleagueSessionExpiredException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        // Log and return null for codes without a description.
                        LogDataError("ADVISOR.TYPES", at.ValInternalCodeAssocMember, null, e, string.Format("Failed to add advisor type {0}", at.ValInternalCodeAssocMember));
                        return null;
                    }
                }
                 , bypassCache: ignoreCache);
            // Exclude nulls from codes without a description.
            return advisorTypes.Where(at => at != null).ToList();
        }

        /// <summary>
        /// Get guid for AdvisorTypesGuid code
        /// </summary>
        /// <param name="code">AdmissionResidencyTypes code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetAdvisorTypeGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetAdvisorTypesAsync(false);
            AdvisorType codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAdvisorTypesAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ADVISOR.TYPES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ADVISOR.TYPES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ADVISOR.TYPES', Record ID:'", code, "'"));
            }
            return guid;
        }

        public async Task<IEnumerable<AdmittedStatus>> GetAdmittedStatusesAsync()
        {
            return await GetCodeItemAsync<AdmitStatuses, AdmittedStatus>("AllAdmitStatuses", "ADMIT.STATUSES",
                a => new AdmittedStatus(a.Recordkey, a.AdmsDesc, a.AdmsTransferFlag));
        }

        public async Task<IEnumerable<Affiliation>> GetAffiliationsAsync()
        {
            var affiliations = await GetOrAddToCacheAsync<List<Affiliation>>("AllAffiliations",
                async () =>
                {
                    Collection<CampusOrgs> affiliationData = await DataReader.BulkReadRecordAsync<CampusOrgs>("CAMPUS.ORGS", "");
                    List<Affiliation> affiliationsList = new List<Affiliation>();
                    foreach (var affiliation in affiliationData)
                    {
                        var orgType = (await CampusOrgTypesAsync()).Where(o => o.Code == affiliation.CmpOrgType).FirstOrDefault();
                        if (orgType != null && orgType.PilotFlag == true)
                            try
                            {
                                affiliationsList.Add(new Affiliation(affiliation.Recordkey, affiliation.CmpDesc));
                            }
                            catch (Exception e)
                            {
                                LogDataError("CAMPUS.ORGS", affiliation.Recordkey, null, e, string.Format("Failed to add affiliation {0}", affiliation.Recordkey));
                            }
                    }
                    return affiliationsList;
                }
            );
            return affiliations;
        }

        public async Task<IEnumerable<ApplicationInfluence>> GetApplicationInfluencesAsync(bool ignoreCache = false)
        {
             return await GetGuidValcodeAsync<ApplicationInfluence>("ST", "APPL.INFLUENCES",
                 (cl, g) => new ApplicationInfluence(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                     ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for ApplicationInfluence code
        /// </summary>
        /// <param name="code">ApplicationInfluence code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetApplicationInfluenceGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetApplicationInfluencesAsync(false);
            ApplicationInfluence codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetApplicationInfluencesAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST-VALCODES, APPLICATION.INFLUENCES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST-VALCODES, APPLICATION.INFLUENCES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST-VALCODES, APPLICATION.INFLUENCES', Record ID:'", code, "'"));
            }
            return guid;

        }

        /// <summary>
        /// Get a collection of Application Sources
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of Application Sources</returns>
        public async Task<IEnumerable<ApplicationSource>> GetApplicationSourcesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<ApplicationSource>("ST", "APPLICATION.SOURCES",
                (es, g) => new ApplicationSource(g, es.ValInternalCodeAssocMember,
                    (string.IsNullOrEmpty(es.ValExternalRepresentationAssocMember)
                    ? es.ValInternalCodeAssocMember : es.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);

        }

        /// <summary>
        /// Get guid for ApplicationSource code
        /// </summary>
        /// <param name="code">ApplicationSource code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetApplicationSourcesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetApplicationSourcesAsync(false);
            ApplicationSource codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetApplicationSourcesAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'APPLICATION.SOURCES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'APPLICATION.SOURCES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'APPLICATION.SOURCES', Record ID:'", code, "'"));
            }
            return guid;

        }

        public async Task<IEnumerable<ApplicationStatus>> GetApplicationStatusesAsync()
        {
            return await GetCodeItemAsync<ApplicationStatuses, ApplicationStatus>("AllApplicationStatuses", "APPLICATION.STATUSES",
                a => new ApplicationStatus(a.Recordkey, a.AppsDesc, a.AppsSpecialProcessingCode));
        }

        public async Task<IEnumerable<ApplicationStatusCategory>> GetApplicationStatusCategoriesAsync()
        {
            return await GetValcodeAsync<ApplicationStatusCategory>("ST", "APPL.STATUS.CONTROLS", applicationStatusCategory => new ApplicationStatusCategory(applicationStatusCategory.ValInternalCodeAssocMember, applicationStatusCategory.ValExternalRepresentationAssocMember));
        }


        /// <summary>
        /// Get guid for AdmissionDecisionTypes
        /// </summary>
        /// <param name="code">AdmissionDecisionTypes code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetAdmissionDecisionTypesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetAdmissionDecisionTypesAsync(false);
            AdmissionDecisionType codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAdmissionDecisionTypesAsync(true);
                if (allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'APPLICATION.STATUSES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'APPLICATION.STATUSES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'APPLICATION.STATUSES', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// Get special processing code for AdmissionDecisionTypes
        /// </summary>
        /// <param name="code">AdmissionDecisionTypes code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetAdmissionDecisionTypesSPCodeAsync(string code)
        {
            //get all the codes from the cache
            string spCode = string.Empty;
            if (string.IsNullOrEmpty(code))
                return spCode;
            var allCodesCache = await GetAdmissionDecisionTypesAsync(false);
            AdmissionDecisionType codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAdmissionDecisionTypesAsync(true);
                if (allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No SpecialProcessingCode found, Entity:'APPLICATION.STATUSES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.SpecialProcessingCode))
                    spCode = codeNoCache.SpecialProcessingCode;
                else
                    throw new RepositoryException(string.Concat("No SpecialProcessingCode found, Entity:'APPLICATION.STATUSES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.SpecialProcessingCode))
                    spCode = codeCache.SpecialProcessingCode;
                else
                    throw new RepositoryException(string.Concat("No SpecialProcessingCode found, Entity:'APPLICATION.STATUSES', Record ID:'", code, "'"));
            }
            return spCode;
        }
        /// <summary>
        /// Get a collection of admission decisions
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of admission decisions</returns>
        public async Task<IEnumerable<AdmissionDecisionType>> GetAdmissionDecisionTypesAsync(bool ignoreCache = false)
        {
            if (ignoreCache)
            {
                return await BuildAllAdmissionDecisionTypes();
            }
            else
            {
                return await GetOrAddToCacheAsync<IEnumerable<AdmissionDecisionType>>("AllAdmissionDecisions", async () => await this.BuildAllAdmissionDecisionTypes(), Level1CacheTimeoutValue);
            }
        }

        private async Task<IEnumerable<AdmissionDecisionType>> BuildAllAdmissionDecisionTypes()
        {
            var admissionDecisionEntities = new List<AdmissionDecisionType>();
            var admissionDecisionIds = await DataReader.SelectAsync("APPLICATION.STATUSES", "WITH APP.INTG.ADM.DEC.TYP.IDX NE ''");

            var admissionDecisionRecords = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.ApplicationStatuses>(admissionDecisionIds);

            foreach (var admissionDecisionRecord in admissionDecisionRecords)
            {
                var admissionDecisionGuidInfo = await GetGuidFromRecordInfoAsync("APPLICATION.STATUSES", admissionDecisionRecord.Recordkey, "APP.INTG.ADM.DEC.TYP.IDX", admissionDecisionRecord.AppIntgAdmDecTypIdx);                    
                admissionDecisionEntities.Add(new AdmissionDecisionType(admissionDecisionGuidInfo, admissionDecisionRecord.Recordkey, !string.IsNullOrEmpty(admissionDecisionRecord.AppsDesc) ? admissionDecisionRecord.AppsDesc : admissionDecisionRecord.Recordkey)
                { AdmissionApplicationStatusTypesCategory = GetAdmissionApplicationType2(admissionDecisionRecord.AppsSpecialProcessingCode), SpecialProcessingCode = admissionDecisionRecord.AppsSpecialProcessingCode });
            }
            return admissionDecisionEntities;
        }

        /// <summary>
        /// Get an admission decision type
        /// </summary>
        /// <param name="guid">admission decision type GUID</param>
        /// <returns>admission decision type</returns>
        public async Task<AdmissionDecisionType> GetAdmissionDecisionTypeByGuidAsync(string guid)
        {
            GuidLookupResult lookupResult = await GetGuidLookupResultFromGuidAsync(guid);
            ApplicationStatuses applicationStatus = await DataReader.ReadRecordAsync<ApplicationStatuses>(lookupResult.PrimaryKey, true);
            return new AdmissionDecisionType(guid, applicationStatus.Recordkey, !string.IsNullOrEmpty(applicationStatus.AppsDesc) ? applicationStatus.AppsDesc : applicationStatus.Recordkey)
            { AdmissionApplicationStatusTypesCategory = GetAdmissionApplicationType2(applicationStatus.AppsSpecialProcessingCode), SpecialProcessingCode = applicationStatus.AppsSpecialProcessingCode };
        }

        ///// <summary>
        ///// Gets & caches application decision types EEDM v11
        ///// </summary>
        ///// <param name="bypassCache"></param>
        ///// <returns></returns>
        //public async Task<IEnumerable<AdmissionDecisionType>> GetAdmissionDecisionTypesAsync(bool bypassCache)
        //{
        //    return await GetGuidCodeItemAsync<ApplicationStatuses, AdmissionDecisionType>("AllApplicationStatusesEEDM", "APPLICATION.STATUSES",
        //        (r, g) => new AdmissionDecisionType(g, r.Recordkey, string.IsNullOrEmpty(r.AppsDesc) ? r.Recordkey : r.AppsDesc)
        //        { AdmissionApplicationStatusTypesCategory = GetAdmissionApplicationType2(r.AppsSpecialProcessingCode), SpecialProcessingCode = r.AppsSpecialProcessingCode },
        //        bypassCache: bypassCache);
        //}

        /// <summary>
        /// Get a collection of assessment special circumstances
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of assessment special circumstances</returns>
        public async Task<IEnumerable<AssessmentSpecialCircumstance>> GetAssessmentSpecialCircumstancesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<AssessmentSpecialCircumstance>("ST", "NON.COURSE.FACTORS",
                (asc, g) => new AssessmentSpecialCircumstance(g, asc.ValInternalCodeAssocMember, asc.ValExternalRepresentationAssocMember), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for AssessmentSpecialCircumstance code
        /// </summary>
        /// <param name="code">AssessmentSpecialCircumstance code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetAssessmentSpecialCircumstancesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetAssessmentSpecialCircumstancesAsync(false);
            AssessmentSpecialCircumstance codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAssessmentSpecialCircumstancesAsync(true);
                if (allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST-VALCODES, NON.COURSE.FACTORS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST-VALCODES, NON.COURSE.FACTORS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST-VALCODES, NON.COURSE.FACTORS', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// Get a collection of BookOptions
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<BookOption>> GetBookOptionsAsync()
        {
            return await GetValcodeAsync<BookOption>("ST", "BOOK.OPTION",
                op => new BookOption(op.ValInternalCodeAssocMember, op.ValExternalRepresentationAssocMember, op.ValActionCode1AssocMember =="1"));
        }

        /// <summary>
        /// Gets CampusInvolvementRoles
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<CampusInvRole>> GetCampusInvolvementRolesAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<Roles, CampusInvRole>("AllCampusInvolvementRoles", "ROLES",
            (r, g) => new CampusInvRole(g, r.Recordkey, r.RolesDesc), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Gets CampusOrganizationTypes for Campus Organization
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<CampusOrganizationType>> GetCampusOrganizationTypesAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<OrgTypes, CampusOrganizationType>("AllOrganizationTypes", "ORG.TYPES",
            (o, g) => new CampusOrganizationType(g, o.Recordkey, o.OrgtDesc), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of FacultySpecialStatuses
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of FacultySpecialStatuses</returns>
        public async Task<IEnumerable<FacultySpecialStatuses>> GetFacultySpecialStatusesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<FacultySpecialStatuses>("ST", "FACULTY.SPECIAL.STATUSES",
                (e, g) => new FacultySpecialStatuses(g, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of FacultyContractTypes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of FacultyContractTypes</returns>
        public async Task<IEnumerable<FacultyContractTypes>> GetFacultyContractTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<FacultyContractTypes>("ST", "FACULTY.CONTRACT.TYPES",
                (e, g) => new FacultyContractTypes(g, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember), bypassCache: ignoreCache);
        }

        public async Task<IEnumerable<CampusOrgType>> CampusOrgTypesAsync()
        {
            return await GetCodeItemAsync<OrgTypes, CampusOrgType>("AllOrgTypes", "ORG.TYPES",
                o => new CampusOrgType(o.Recordkey, o.OrgtDesc, (o.OrgtPilotFlag == "Y" ? true : false)));
        }

        public async Task<IEnumerable<CampusOrgRole>> CampusOrgRolesAsync()
        {
            return await GetCodeItemAsync<Roles, CampusOrgRole>("AllOrgRoles", "ROLES",
                r => new CampusOrgRole(r.Recordkey, r.RolesDesc, r.RolesPilotPriority));
        }

        public async Task<IEnumerable<CareerGoal>> GetCareerGoalsAsync(bool ignoreCache = false)
        {
       
            return await GetGuidValcodeAsync<CareerGoal>("ST", "CAREER.GOALS",
               (cl, g) => new CareerGoal(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                   ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);

        }

        
        /// <summary>
        /// Get guid for Career Goal code
        /// </summary>
        /// <param name="code">Career Goal code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetCareerGoalGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetCareerGoalsAsync(false);
            CareerGoal codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetCareerGoalsAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST-VALCODES, CAREER.GOALS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST-VALCODES, CAREER.GOALS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST-VALCODES, CAREER.GOALS', Record ID:'", code, "'"));
            }
            return guid;

        }
                
        public async Task<IEnumerable<Ccd>> GetCcdsAsync()
        {
            return await GetCodeItemAsync<Ccds, Ccd>("AllCcds", "CCDS",
                ccd => new Ccd(ccd.Recordkey, ccd.CcdDesc));
        }

        /// <summary>
        /// Get a collection of ChargeAssessmentMethod
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of ChargeAssessmentMethod</returns>
        public async Task<IEnumerable<ChargeAssessmentMethod>> GetChargeAssessmentMethodsAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<ChargeAssessmentMethod>("ST", "BILLING.METHODS",
                (cl, g) => new ChargeAssessmentMethod(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }

        public async Task<IEnumerable<CourseLevel>> GetCourseLevelsAsync()
        {
            return await GetCourseLevelsAsync(false);
        }

        public async Task<IEnumerable<ClassLevel>> GetClassLevelsAsync()
        {
            return await GetCodeItemAsync<Classes, ClassLevel>("AllClassLevels", "CLASSES",
                classLevel => new ClassLevel(classLevel.Recordkey, classLevel.ClsDesc, classLevel.ClsSortOrder));
        }

        /// <summary>
        /// Get a collection of course levels
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of course levels</returns>
        public async Task<IEnumerable<CourseLevel>> GetCourseLevelsAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<CourseLevel>("ST", "COURSE.LEVELS",
                (cl, g) => new CourseLevel(g, cl.ValInternalCodeAssocMember, cl.ValExternalRepresentationAssocMember), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for CourseLevel code
        /// </summary>
        /// <param name="code">CourseLevel code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetCourseLevelGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetCourseLevelsAsync(false);
            CourseLevel codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetCourseLevelsAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST-VALCODES, COURSE.LEVELS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST-VALCODES, COURSE.LEVELS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST-VALCODES, COURSE.LEVELS', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// Get a collection of education goals.
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<EducationGoals>> GetEducationGoalsAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<EducationGoals>("ST", "EDUCATION.GOALS",
                (cl, g) => new EducationGoals(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for education goal code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<string> GetEducationGoalGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetEducationGoalsAsync(false);
            EducationGoals codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetEducationGoalsAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST.VALCODES - EDUCATION.GOALS ', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST.VALCODES - EDUCATION.GOALS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST.VALCODES - EDUCATION.GOALS', Record ID:'", code, "'"));
            }
            return guid;

        }

        public async Task<IEnumerable<CourseStatuses>> GetCourseStatusesAsync()
        {
            return await GetCourseStatusesAsync(false);
        }

        /// <summary>
        /// Get a collection of CourseStatuses
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of CourseStatuses</returns>
        public async Task<IEnumerable<CourseStatuses>> GetCourseStatusesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<CourseStatuses>("ST", "COURSE.STATUSES",
                (cl, g) => new CourseStatuses(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)) { Status = GetCourseStatus(cl.ValActionCode1AssocMember) }, bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for CourseStatus code
        /// </summary>
        /// <param name="code">CourseStatus code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetCourseStatusGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetCourseStatusesAsync(false);
            CourseStatuses codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetCourseStatusesAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST.VALCODES - COURSE.STATUSES ', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST.VALCODES - COURSE.STATUSES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST.VALCODES - COURSE.STATUSES', Record ID:'", code, "'"));
            }
            return guid;

        }

        /// <summary>
        /// Get a collection of course title types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of course title types</returns>
        public async Task<IEnumerable<CourseTitleType>> GetCourseTitleTypesAsync(bool ignoreCache = false)
        {
            return await GetGuidCodeItemAsync<IntgCrsTitleTypes, CourseTitleType>("AllCourseTitleTypes", "INTG.CRS.TITLE.TYPES",
                (c, g) => new CourseTitleType(g, c.Recordkey, c.IcttDesc) { Title = c.IcttTitle }, bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for CourseTitleType code
        /// </summary>
        /// <param name="code">CourseTitleType code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetCourseTitleTypeGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetCourseTitleTypesAsync(false);
            CourseTitleType codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetCourseTitleTypesAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'INTG.CRS.TITLE.TYPES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'INTG.CRS.TITLE.TYPES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'INTG.CRS.TITLE.TYPES', Record ID:'", code, "'"));
            }
            return guid;

        }

        /// <summary>
        /// Get a collection of course topics
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of course topics</returns>
        public async Task<IEnumerable<CourseTopic>> GetCourseTopicsAsync(bool ignoreCache = false)
        {
            return await GetGuidCodeItemAsync<TopicCodes, CourseTopic>("AllCourseTopics", "TOPIC.CODES",
                (c, g) => new CourseTopic(g, c.Recordkey, c.TopcDesc), bypassCache: ignoreCache);
        }        

        /// <summary>
        /// Get a collection of CourseTypes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of CourseTypes</returns>
        public async Task<IEnumerable<CourseType>> GetCourseTypesAsync(bool ignoreCache = false)
        {
            return await GetGuidValcodeAsync<CourseType>("ST", "COURSE.TYPES",
                (cl, g) => new CourseType(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember),
                    !string.IsNullOrEmpty(cl.ValActionCode2AssocMember) ? cl.ValActionCode2AssocMember.ToUpper() != "N" : true), 
                bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for CourseType code
        /// </summary>
        /// <param name="code">CourseTypes code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetCourseTypeGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetCourseTypesAsync(false);
            CourseType codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetCourseTypesAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST.VALCODES - COURSE.TYPES ', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST.VALCODES - COURSE.TYPES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST.VALCODES - COURSE.TYPES', Record ID:'", code, "'"));
            }
            return guid;

        }

        public async Task<IEnumerable<CredType>> GetCreditTypesAsync()
        {
            return await GetCodeItemAsync<CredTypes, CredType>("AllCredTypes", "CRED.TYPES",
                c => new CredType(c.Recordkey, c.CrtpDesc)
                {
                    Category = (c.CrtpCategory == "I" ? CreditType.Institutional :
                        c.CrtpCategory == "T" ? CreditType.Transfer :
                        c.CrtpCategory == "C" ? CreditType.ContinuingEducation :
                        CreditType.Other)
                });
        }

        public async Task<IEnumerable<CreditCategory>> GetCreditCategoriesAsync()
        {
            return await GetCreditCategoriesAsync(false);
        }

        /// <summary>
        /// Get a collection of credit categories
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of credit categories</returns>
        public async Task<IEnumerable<CreditCategory>> GetCreditCategoriesAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<CredTypes, CreditCategory>("AllCreditCategories", "CRED.TYPES",
                (ct, g) => new CreditCategory(g, ct.Recordkey, ct.CrtpDesc, GetCreditType(ct.CrtpCategory)) { Category = ct.CrtpCategory}, bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for CreditCategories code
        /// </summary>
        /// <param name="code">CreditCategories code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetCreditCategoriesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetCreditCategoriesAsync(false);
            CreditCategory codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetCreditCategoriesAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CRED.TYPES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CRED.TYPES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CRED.TYPES', Record ID:'", code, "'"));
            }
            return guid;

        }

        public async Task<IEnumerable<Degree>> GetDegreesAsync()
        {
            return await GetCodeItemAsync<Degrees, Degree>("AllDegrees", "DEGREES",
                m => new Degree(m.Recordkey, m.DegDesc));
        }

        /// <summary>
        /// Get a collection of enrollment statuses
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of enrollment statuses</returns>
        public async Task<IEnumerable<EnrollmentStatus>> GetEnrollmentStatusesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<EnrollmentStatus>("ST", "STUDENT.PROGRAM.STATUSES",
                (es, g) => new EnrollmentStatus(g, es.ValInternalCodeAssocMember, es.ValExternalRepresentationAssocMember,
                    GetEnrollmentStatusType(es.ValActionCode1AssocMember)), bypassCache: ignoreCache);
        }
        public async Task<IEnumerable<FederalCourseClassification>> GetFederalCourseClassificationsAsync()
        {
            return await GetCodeItemAsync<Cip, FederalCourseClassification>("AllFederalCourseClassifications", "CIP",
                c => new FederalCourseClassification(c.Recordkey, c.CipDesc));
        }

        public async Task<IEnumerable<LocalCourseClassification>> GetLocalCourseClassificationsAsync()
        {
            return await GetCodeItemAsync<LocalGovtCodes, LocalCourseClassification>("AllLocalCourseClassifications", "LOCAL.GOVT.CODES",
                l => new LocalCourseClassification(l.Recordkey, l.LgcDesc));
        }

        public async Task<IEnumerable<ExternalTranscriptStatus>> GetExternalTranscriptStatusesAsync()
        {
            return await GetValcodeAsync<ExternalTranscriptStatus>("ST", "EXTL.TRAN.STATUSES", externalTranscriptStatus => new ExternalTranscriptStatus(externalTranscriptStatus.ValInternalCodeAssocMember, externalTranscriptStatus.ValExternalRepresentationAssocMember));
        }

        public async Task<IEnumerable<GradeScheme>> GetGradeSchemesAsync()
        {
            return await GetGradeSchemesAsync(false);
        }

        /// <summary>
        /// Get a collection of grade schemes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of grade schemes</returns>
        public async Task<IEnumerable<GradeScheme>> GetGradeSchemesAsync( bool ignoreCache )
        {
            if( ignoreCache )
            {
                return await AddOrUpdateCacheAsync<IEnumerable<GradeScheme>>( "AllGradeSchemes", await GetGradeSchemeEntitiesAsync() );
            }
            else
            {
                return await GetOrAddToCacheAsync( "AllGradeSchemes", async () => { return await GetGradeSchemeEntitiesAsync(); } );
            }
        }

        private async Task<List<GradeScheme>> GetGradeSchemeEntitiesAsync()
        {
            Collection<GradeSchemes> gradeSchemesData = await DataReader.BulkReadRecordAsync<GradeSchemes>( "" );
            var gradeSchemeList = BuildGradeSchemes( gradeSchemesData );
            return gradeSchemeList;
        }

        /// <summary>
        /// Convert a list of Grade Schemes data contract objects to a list of <see cref="GradeScheme"/> entity objects
        /// </summary>
        /// <param name="gradeSchemeData"> The list of GradeSchemes objects to convert</param>
        /// <returns></returns>

        private List<GradeScheme> BuildGradeSchemes(Collection<GradeSchemes> gradeSchemeData)
        {
            var gradeSchemes = new List<GradeScheme>();
            // If no data passed in, return a null collection
            if (gradeSchemeData != null)
            {
                foreach (var grdScheme in gradeSchemeData)
                {
                    try
                    {
                        var gradeScheme = new GradeScheme( grdScheme.RecordGuid, grdScheme.Recordkey, grdScheme.GrsDesc);
                        gradeScheme.EffectiveStartDate = grdScheme.GrsStartDate;
                        gradeScheme.EffectiveEndDate = grdScheme.GrsEndDate;
                        if (grdScheme.GrsGrades != null && grdScheme.GrsGrades.Any())
                        {
                            grdScheme.GrsGrades.ForEach(gs => gradeScheme.AddGradeCode(gs));
                        }
                        gradeSchemes.Add(gradeScheme);
                    }
                    catch (Exception ex)
                    {
                        LogDataError("GradeScheme", grdScheme.Recordkey, grdScheme, ex);
                    }

                }
            }
            return gradeSchemes;
        }

        /// <summary>
        /// Get a collection of grade subschemes
        /// </summary>
        /// <returns>Collection of grade subschemes</returns>
        public async Task<IEnumerable<GradeSubscheme>> GetGradeSubschemesAsync()
        {
            var gradeSubschemes = await GetOrAddToCacheAsync<List<GradeSubscheme>>("AllGradeSubschemes",
                async () =>
                {
                    Collection<GradeSubschemes> gradeSubschemesData = await DataReader.BulkReadRecordAsync<GradeSubschemes>("");
                    var gradeSubschemeList = BuildGradeSubschemes(gradeSubschemesData);
                    return gradeSubschemeList;
                }
            );
            return gradeSubschemes;
        }

        /// <summary>
        /// Convert a list of Grade Subschemes data contract objects to a list of <see cref="GradeSubscheme"/> entity objects
        /// </summary>
        /// <param name="gradeSubschemesData"> The list of GradeSubschemes objects to convert</param>
        /// <returns></returns>

        private List<GradeSubscheme> BuildGradeSubschemes(Collection<GradeSubschemes> gradeSubschemesData)
        {
            var gradeSubschemes = new List<GradeSubscheme>();
            // If no data passed in, return a null collection
            if (gradeSubschemesData != null)
            {
                foreach (var grdSubscheme in gradeSubschemesData)
                {
                    try
                    {
                        var gradeSubscheme = new GradeSubscheme(grdSubscheme.Recordkey, grdSubscheme.GssDescription);
                        if (grdSubscheme.GssGrades != null && grdSubscheme.GssGrades.Any())
                        {
                            grdSubscheme.GssGrades.ForEach(gs => gradeSubscheme.AddGradeCode(gs));
                        }
                        gradeSubschemes.Add(gradeSubscheme);
                    }
                    catch (Exception ex)
                    {
                        LogDataError("GradeSubscheme", grdSubscheme.Recordkey, grdSubscheme, ex);
                    }

                }
            }
            return gradeSubschemes;
        }


        /// <summary>
        /// Get guid for GradeScheme code
        /// </summary>
        /// <param name="code">GradeScheme code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetGradeSchemeGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetGradeSchemesAsync(false);
            GradeScheme codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetGradeSchemesAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'GRADE.SCHEMES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'GRADE.SCHEMES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'GRADE.SCHEMES', Record ID:'", code, "'"));
            }
            return guid;

        }

        public async Task<IEnumerable<InstructionalMethod>> GetInstructionalMethodsAsync()
        {
            return await GetGuidCodeItemAsync<InstrMethods, InstructionalMethod>("AllInstructionalMethods", "INSTR.METHODS",
                (i, g) => new InstructionalMethod(g, i.Recordkey, i.InmDesc, i.InmOnline == "Y"));
        }

        /// <summary>
        /// Get a collection of instructional methods
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of instructional methods</returns>
        public async Task<IEnumerable<InstructionalMethod>> GetInstructionalMethodsAsync(bool ignoreCache)
        {
            if (ignoreCache)
            {
                return await BuildAllInstructionalMethods();
            }
            else
            {
                return await GetOrAddToCacheAsync<IEnumerable<InstructionalMethod>>("AllInstructionalMethods", async () => await this.BuildAllInstructionalMethods(), Level1CacheTimeoutValue);
            }
        }

        /// <summary>
        /// Get guid for InstructionalMethod code
        /// </summary>
        /// <param name="code">InstructionalMethod code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetInstructionalMethodGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetInstructionalMethodsAsync(false);
            InstructionalMethod codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetInstructionalMethodsAsync(true);
                if (allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'INSTR.METHODS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'INSTR.METHODS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'INSTR.METHODS', Record ID:'", code, "'"));
            }
            return guid;

        }

        private async Task<IEnumerable<InstructionalMethod>> BuildAllInstructionalMethods()
        {
            var instructionalMethodEntities = new List<InstructionalMethod>();
            var instructionalMethodRecords = await DataReader.BulkReadRecordAsync<InstrMethods>("INSTR.METHODS", "");

            foreach (var record in instructionalMethodRecords)
            {
                if (string.IsNullOrEmpty(record.RecordGuid))
                {
                    throw new ArgumentNullException("guid", string.Format("No Guid found, Entity:'INSTR.METHODS', Record ID:'{0}'", record.Recordkey));
                }
                instructionalMethodEntities.Add(new InstructionalMethod(record.RecordGuid, record.Recordkey, !string.IsNullOrEmpty(record.InmDesc) ? record.InmDesc : record.Recordkey, record.InmOnline == "Y"));
            }
            return instructionalMethodEntities;
        }

        /// <summary>
        /// Get a collection of administrative instructional methods
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of administrative instructional methods</returns>
        public async Task<IEnumerable<AdministrativeInstructionalMethod>> GetAdministrativeInstructionalMethodsAsync(bool ignoreCache = false)
        {
            if (ignoreCache)
            {
                return await BuildAllAdministrativeInstructionalMethods();
            }
            else
            {
                return await GetOrAddToCacheAsync<IEnumerable<AdministrativeInstructionalMethod>>("AllAdministrativeInstructionalMethods", async () => await this.BuildAllAdministrativeInstructionalMethods(), Level1CacheTimeoutValue);
            }
        }

        /// <summary>
        /// Get guid for AdministrativeInstructionalMethod code
        /// </summary>
        /// <param name="code">AdministrativeInstructionalMethod code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetAdministrativeInstructionalMethodGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetAdministrativeInstructionalMethodsAsync(false);
            AdministrativeInstructionalMethod codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAdministrativeInstructionalMethodsAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'INSTR.METHODS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'INSTR.METHODS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'INSTR.METHODS', Record ID:'", code, "'"));
            }
            return guid;
        }

        private async Task<IEnumerable<AdministrativeInstructionalMethod>> BuildAllAdministrativeInstructionalMethods()
        {
            var administrativeInstructionalMethodEntities = new List<AdministrativeInstructionalMethod>();
            var administrativeInstructionalMethodIds = await DataReader.SelectAsync("INSTR.METHODS", null);

            var administrativeInstructionalMethodRecords = await DataReader.BulkReadRecordAsync<InstrMethods>(administrativeInstructionalMethodIds);

            foreach (var record in administrativeInstructionalMethodRecords)
            {
                string guid = string.Empty;
                // The method may throw an error but we want to catch it and send a proper error message.
                try
                {
                    guid = await GetGuidFromRecordInfoAsync("INSTR.METHODS", record.Recordkey, "INM.INTG.KEY.IDX", record.InmIntgKeyIdx);
                }
                catch
                {
                    guid = string.Empty;
                }
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("RecordGuid", string.Format("The secondary GUID is missing for INSTR.METHODS entity '{0}'.", record.Recordkey));
                }
                if (string.IsNullOrEmpty(record.RecordGuid))
                {
                    throw new ArgumentNullException("RecordGuid", string.Format("The primary GUID is missing for INSTR.METHODS entity '{0}'.", record.Recordkey));
                }
                administrativeInstructionalMethodEntities.Add(new AdministrativeInstructionalMethod(guid, record.Recordkey, !string.IsNullOrEmpty(record.InmDesc) ? record.InmDesc : record.Recordkey, record.RecordGuid));

            }


            return administrativeInstructionalMethodEntities;
        }

        public async Task<IEnumerable<Major>> GetMajorsAsync(bool ignoreCache = false)
        {
            return await GetCodeItemAsync<Majors, Major>("AllMajors", "MAJORS",
                m => new Major(m.Recordkey, m.MajDesc)
                {
                    DivisionCode = m.MajDivision,
                    ActiveFlag = (m.MajActiveFlag == "Y" ? true : false),
                    FederalCourseClassification = m.MajCip,
                    LocalCourseClassifications = m.MajLocalGovtCodes
                }, bypassCache: ignoreCache);
        }


        /// <summary>
        /// Get a collection of MealPlanRates
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of MealPlanRates domain entities</returns>
        public async Task<IEnumerable<MealPlanRates>> GetMealPlanRatesAsync(bool ignoreCache)
        {
            if (ignoreCache)
            {
                var mealPlanRates = await BuildAllMealPlanRates();
                return await AddOrUpdateCacheAsync<IEnumerable<MealPlanRates>>("AllMealPlanRates", mealPlanRates);
            }
            else
            {
                return await GetOrAddToCacheAsync<IEnumerable<MealPlanRates>>("AllMealPlanRates", async () => await this.BuildAllMealPlanRates(), Level1CacheTimeoutValue);
            }
        }


        private async Task<IEnumerable<MealPlanRates>> BuildAllMealPlanRates()
        {
            var mealPlanRateEntities = new List<MealPlanRates>();
            var mealPlanIds = await DataReader.SelectAsync("MEAL.PLANS", "");
            var mealPlanRecords = await DataReader.BulkReadRecordAsync<DataContracts.MealPlans>(mealPlanIds);

            foreach (var mealPlanRecord in mealPlanRecords)
            {
                if (mealPlanRecord.MealPlanRatesEntityAssociation != null && mealPlanRecord.MealPlanRatesEntityAssociation.Any())
                {
                    var effectiveDate = DateTime.MinValue;
                    try
                    {
                        string mealPlanGuidInfo = string.Empty;

                        foreach (var mealPlanRate in mealPlanRecord.MealPlanRatesEntityAssociation)
                        {

                            effectiveDate = Convert.ToDateTime(mealPlanRate.MealRateEffectiveDatesAssocMember);
                            //convert a datetime to a unidata internal value 
                            var offsetDate = DmiString.DateTimeToPickDate(effectiveDate);

                            mealPlanGuidInfo = await GetGuidFromRecordInfoAsync("MEAL.PLANS", mealPlanRecord.Recordkey, "MEAL.RATE.EFFECTIVE.DATES", offsetDate.ToString());

                            mealPlanRateEntities.Add(new MealPlanRates(mealPlanGuidInfo, mealPlanRecord.Recordkey, mealPlanRecord.MealDesc)
                            {

                                MealArCode = mealPlanRecord.MealArCode,
                                MealRatePeriod = GetMealPlanRatePeriods(mealPlanRecord.MealRatePeriod),
                                MealPlansMealPlanRates = new Domain.Student.Entities.MealPlansMealPlanRates(mealPlanRate.MealRatesAssocMember, mealPlanRate.MealRateEffectiveDatesAssocMember)

                            });
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        throw new ColleagueWebApiException(string.Concat(ex.Message, ", effectiveDate: ", effectiveDate != DateTime.MinValue ? effectiveDate.ToShortDateString() : ""),
                            ex.InnerException);
                    }
                }
            }

            return mealPlanRateEntities;
        }

        /// <summary>
        /// Get a collection of IntgTestPercentileTypes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of IntgTestPercentileTypes</returns>
        public async Task<IEnumerable<IntgTestPercentileType>> GetIntgTestPercentileTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<IntgTestPercentileType>("ST", "INTG.TEST.PERCENTILE.TYPES",
                (e, g) => new IntgTestPercentileType(g, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for IntgTestPercentileTypes code
        /// </summary>
        /// <param name="code">IntgTestPercentileTypes code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetIntgTestPercentileTypesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetIntgTestPercentileTypesAsync(false);
            IntgTestPercentileType codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetIntgTestPercentileTypesAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST-VALCODES, INTG.TEST.PERCENTILE.TYPES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST-VALCODES, INTG.TEST.PERCENTILE.TYPES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST-VALCODES, INTG.TEST.PERCENTILE.TYPES', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// Get a collection of meal plans
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of meal plans</returns>
        public async Task<IEnumerable<MealPlan>> GetMealPlansAsync(bool ignoreCache = false)
        {
            return await GetGuidCodeItemAsync<MealPlans, MealPlan>("AllMealPlans", "MEAL.PLANS",
                (m, g) =>
                {
                    return new MealPlan(g, m.Recordkey, m.MealDesc)
                    {
                        Classification = m.MealClass,
                        ComponentNumberOfUnits = m.MealNoTimes,
                        ComponentUnitType = "meal",
                        ComponentTimePeriod = m.MealFrequency,
                        MealTypes = m.MealTypes,
                        DiningFacilities = m.MealRooms,
                        StartDay = m.MealStartDay,
                        EndDay = m.MealEndDay,
                        Buildings = m.MealBldgs,
                        Sites = m.MealLocations,
                        StartDate = m.MealStartDate,
                        EndDate = m.MealEndDate,
                        RatePeriod = m.MealRatePeriod
                    };
                }, bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for MealPlan code
        /// </summary>
        /// <param name="code">MealPlan code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetMealPlanGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetMealPlansAsync(false);
            MealPlan codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetMealPlansAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'MEAL.PLANS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'MEAL.PLANS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'MEAL.PLANS', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// Get a collection of MealType
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of MealType</returns>
        public async Task<IEnumerable<MealType>> GetMealTypesAsync(bool ignoreCache = false)
        {
            return await GetGuidValcodeAsync<MealType>("ST", "MEAL.TYPE",
                (m, g) => new MealType(g, m.ValInternalCodeAssocMember, m.ValExternalRepresentationAssocMember), bypassCache: ignoreCache);
        }

        public async Task<IEnumerable<Minor>> GetMinorsAsync(bool ignoreCache = false)
        {
            return await GetCodeItemAsync<Minors, Minor>("AllMinors", "MINORS",
                m => new Minor(m.Recordkey, m.MinorsDesc)
                {
                    FederalCourseClassification = m.MinorsCip
                }, bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of NonCourseCategories
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of NonCourseCategories</returns>
        public async Task<IEnumerable<NonCourseCategories>> GetNonCourseCategoriesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<NonCourseCategories>("ST", "NON.COURSE.CATEGORIES",
                (e, g) => new NonCourseCategories(g, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember) { SpecialProcessingCode = e.ValActionCode1AssocMember }, bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of NonCourseGradeUses
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of NonCourseGradeUses</returns>
        public async Task<IEnumerable<NonCourseGradeUses>> GetNonCourseGradeUsesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<NonCourseGradeUses>("ST", "NON.COURSE.GRADE.USES",
                (e, g) => new NonCourseGradeUses(g, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of HousingResidentType
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of HousingResidentType</returns>
        public async Task<IEnumerable<HousingResidentType>> GetHousingResidentTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<HousingResidentType>("ST", "ROOM.ASSIGN.STAFF.CODES",
                (cl, g) => new HousingResidentType(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of RoomRates
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of RoomRates domain entities</returns>
        public async Task<IEnumerable<RoomRate>> GetRoomRatesAsync(bool ignoreCache)
        {
            if (ignoreCache)
            {
                var roomRates = await BuildAllRoomRates();
                return await AddOrUpdateCacheAsync<IEnumerable<RoomRate>>("AllRoomRates", roomRates);
            }
            else
            {
                return await GetOrAddToCacheAsync<IEnumerable<RoomRate>>("AllRoomRates", async () => await this.BuildAllRoomRates(), Level1CacheTimeoutValue);
            }
        }

        private async Task<IEnumerable<RoomRate>> BuildAllRoomRates()
        {
            var roomRateEntities = new List<RoomRate>();
            var roomRateIds = await DataReader.SelectAsync("ROOM.RATE.TABLES", "");
            var roomRateRecords = await DataReader.BulkReadRecordAsync<DataContracts.RoomRateTables>(roomRateIds);

            foreach (var roomRateRecord in roomRateRecords)
            {
                if (roomRateRecord.RoomDateRatesEntityAssociation != null && roomRateRecord.RoomDateRatesEntityAssociation.Any())
                {
                    var effectiveDate = DateTime.MinValue;
                    try
                    {
                        string roomRateGuidInfo = string.Empty;

                        foreach (var roomRateStartDate in roomRateRecord.RoomDateRatesEntityAssociation)
                        {

                            effectiveDate = Convert.ToDateTime(roomRateStartDate.RrtEffectiveDatesAssocMember);
                            //convert a datetime to a unidata internal value 
                            var offsetDate = DmiString.DateTimeToPickDate(effectiveDate);

                            roomRateGuidInfo = await GetGuidFromRecordInfoAsync("ROOM.RATE.TABLES", roomRateRecord.Recordkey, "RRT.EFFECTIVE.DATES", offsetDate.ToString());

                            roomRateEntities.Add(new RoomRate(roomRateGuidInfo, roomRateRecord.Recordkey, roomRateRecord.RrtDesc)
                            {
                                StartDate = effectiveDate,
                                EndDate = roomRateRecord.RrtEndDate,
                                DayRate = roomRateStartDate.RrtDayRatesAssocMember,
                                WeeklyRate = roomRateStartDate.RrtWeekRatesAssocMember,
                                MonthlyRate = roomRateStartDate.RrtMonthRatesAssocMember,
                                TermRate = roomRateStartDate.RrtTermRatesAssocMember,
                                AnnualRate = roomRateStartDate.RrtYearRatesAssocMember,
                                AccountingCode = !string.IsNullOrEmpty(roomRateRecord.RrtArCode) ? GetGuidFromRecordInfo("AR.CODES", roomRateRecord.RrtArCode) : null,
                                CancelAccountingCode = !string.IsNullOrEmpty(roomRateRecord.RrtCancelArCode) ? GetGuidFromRecordInfo("AR.CODES", roomRateRecord.RrtCancelArCode) : null
                            });
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        throw new ColleagueWebApiException(string.Concat(ex.Message, ", effectiveDate: ", effectiveDate != DateTime.MinValue ? effectiveDate.ToShortDateString() : ""),
                            ex.InnerException);
                    }
                }
            }

            return roomRateEntities;
        }

        public async Task<IEnumerable<SectionRegistrationStatusItem>> SectionRegistrationStatusesAsync()
        {
            //return await GetSectionRegistrationStatusesAsync(false);
            return await GetStudentAcademicCreditStatusesAsync(false);
        }

        public async Task<IEnumerable<SectionGradeType>> GetSectionGradeTypesAsync()
        {
            return await GetSectionGradeTypesAsync(false);
        }

        /// <summary>
        /// Get a collection of section grade types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of section grade types</returns>
        public async Task<IEnumerable<SectionGradeType>> GetSectionGradeTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<SectionGradeType>("ST", "INTG.SECTION.GRADE.TYPES",
                (cl, g) => new SectionGradeType(g, cl.ValInternalCodeAssocMember, cl.ValExternalRepresentationAssocMember), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of Student Academic Credit Statuses
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of student academic credit statuses</returns>
        public async Task<IEnumerable<SectionRegistrationStatusItem>> GetStudentAcademicCreditStatusesAsync(bool ignoreCache = false)
        {
            var exception = new RepositoryException();
            var statusItems = new List<SectionRegistrationStatusItem>();
            var statusList = await GetGuidValcodeAsync<SectionRegistrationStatusItem>("ST", "STUDENT.ACAD.CRED.STATUSES",
                (cs, g) =>
                    {
                        SectionRegistrationStatusItem sectionRegistrationStatus = null;
                        try
                        {
                            sectionRegistrationStatus = new SectionRegistrationStatusItem(g, cs.ValInternalCodeAssocMember,
                            (string.IsNullOrEmpty(cs.ValExternalRepresentationAssocMember) ? cs.ValInternalCodeAssocMember : cs.ValExternalRepresentationAssocMember))
                            {
                                Status = GetSectionRegistrationStatus(cs.ValActionCode1AssocMember),
                                Category = ConvertTransferStatuses(cs.ValActionCode1AssocMember)
                            };
                        }
                        catch (Exception ex)
                        {
                            exception.AddError(new Domain.Entities.RepositoryError("Missing.Required.Property", "Invalid STUDENT.ACAD.CRED.STATUSES table entry. " + ex.Message)
                            {
                                Id = g,
                                SourceId = cs.ValInternalCodeAssocMember
                            });
                        }
                        if (exception != null && exception.Errors != null && exception.Errors.Any())
                        {
                            return null;
                        }
                        return sectionRegistrationStatus;
                    },
                bypassCache: ignoreCache);

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            else
            {
                foreach (var status in statusList)
                {
                    if (status == null)
                    {
                        exception.AddError(new Domain.Entities.RepositoryError("Missing.Required.Property", "Invalid STUDENT.ACAD.CRED.STATUSES table entry. Value cannot be null.  Parameter name: guid"));
                        throw exception;
                    }
                }
            }
            return statusList;
        }

        /// <summary>
        /// Get guid for student Academic Credit Statuses
        /// </summary>
        /// <param name="code">student Academic Credit Statuses</param>
        /// <returns>Guid</returns>
        public async Task<string> GetStudentAcademicCreditStatusesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetStudentAcademicCreditStatusesAsync(false);
            SectionRegistrationStatusItem codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetStudentAcademicCreditStatusesAsync(true);
                if (allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST.VALCODE - STUDENT.ACAD.CRED.STATUSES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST.VALCODE - STUDENT.ACAD.CRED.STATUSES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST.VALCODE - STUDENT.ACAD.CRED.STATUSES', Record ID:'", code, "'"));
            }
            return guid;

        }


        /// <summary>
        /// Return a list of section-registration-statuses that should be included in the headcount enumeration
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns>List of statuses to be included in headcount enumeration</returns>
        public async Task<List<string>> GetHeadcountInclusionListAsync(bool ignoreCache = false)
        {
            var exception = new RepositoryException();
            List<string> headCountInclusionList = new List<string>();
            if (ignoreCache)
            {
                ClearCache(new List<string>() { "LdmDefaultsHeadcountInclusion" });
            }
            headCountInclusionList = await GetOrAddToCacheAsync<List<string>>("LdmDefaultsHeadcountInclusion",
                async () =>
                {
                    var ldmDefaults = await DataReader.ReadRecordAsync<LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS");

                    if (ldmDefaults == null)
                    {
                        exception.AddError(new Domain.Entities.RepositoryError("Missing.Required.Property", "CDM configuration setup not complete")
                        {
                            Id = "CORE.PARMS",
                            SourceId = "LDM.DEFAULTS"
                        });
                        throw exception;
                    }
                    return ldmDefaults.LdmdIncludeEnrlHeadcounts;
                });

            return headCountInclusionList;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a CourseTransferStatusesCategory domain special processing code value to its corresponding CourseTransferStatusesCategory enumeration value
        /// </summary>
        /// <param name="source">CourseTransferStatusesCategory domain special processing code value</param>
        /// <returns>CourseTransferStatusesCategory enumeration value</returns>
        private CourseTransferStatusesCategory ConvertTransferStatuses(string source)
        {
            switch (source)
            {
                case "7":
                    return CourseTransferStatusesCategory.Approved;
                case "8":
                    return CourseTransferStatusesCategory.Preliminary;
                case null:
                    return CourseTransferStatusesCategory.Preliminary;
                case "":
                    return CourseTransferStatusesCategory.Preliminary;
                default:
                    return CourseTransferStatusesCategory.NotSet;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="NonAcademicAttendanceEventType">nonacademic attendance event types</see>
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of <see cref="NonAcademicAttendanceEventType">nonacademic attendance event types</see></returns>
        public async Task<IEnumerable<NonAcademicAttendanceEventType>> GetNonAcademicAttendanceEventTypesAsync(bool ignoreCache = false)
        {
            var nonAcademicAttendanceEventTypes = await GetValcodeAsync<NonAcademicAttendanceEventType>("ST", "NAA.EVENT.TYPES",
                naaet =>
                {
                    try
                    {
                        return new NonAcademicAttendanceEventType(naaet.ValInternalCodeAssocMember, naaet.ValExternalRepresentationAssocMember);

                    }
                    catch (Exception e)
                    {
                        // Log and return null for codes without a description.
                        LogDataError("NAA.EVENT.TYPES", naaet.ValInternalCodeAssocMember, null, e, string.Format("Failed to add nonacademic attendance event type {0}", naaet.ValInternalCodeAssocMember));
                        return null;
                    }
                }
                );
            // Exclude nulls from codes without a description.
            return nonAcademicAttendanceEventTypes.Where(sl => sl != null).ToList();
        }

        /// <summary>
        /// Gets Admission Application Status Type Category
        /// </summary>
        /// <param name="appsSpecialProcessingCode"></param>
        /// <returns></returns>
        private AdmissionApplicationStatusTypesCategory GetAdmissionApplicationType(string appsSpecialProcessingCode)
        {
            switch (appsSpecialProcessingCode.ToUpperInvariant())
            {
                case "AP":
                    return AdmissionApplicationStatusTypesCategory.Submitted;
                case "CO":
                    return AdmissionApplicationStatusTypesCategory.Readyforreview;
                case "AC":
                case "WL":
                case "RE":
                case "WI":
                    return AdmissionApplicationStatusTypesCategory.Decisionmade;
                case "MS":
                    return AdmissionApplicationStatusTypesCategory.Enrollmentcomplete;
                default:
                    return AdmissionApplicationStatusTypesCategory.Started;
            }
        }
        /// <summary>
        /// Gets Admission Application Status Type Category
        /// </summary>
        /// <param name="appsSpecialProcessingCode"></param>
        /// <returns></returns>
        private AdmissionApplicationStatusTypesCategory GetAdmissionApplicationType2(string appsSpecialProcessingCode)
        {
            if (!string.IsNullOrEmpty(appsSpecialProcessingCode))
            {
                switch (appsSpecialProcessingCode.ToUpperInvariant())
                {
                    case "AP":
                        return AdmissionApplicationStatusTypesCategory.Applied;
                    case "CO":
                        return AdmissionApplicationStatusTypesCategory.Complete;
                    case "AC":
                        return AdmissionApplicationStatusTypesCategory.Accepted;
                    case "WL":
                        return AdmissionApplicationStatusTypesCategory.WaitListed;
                    case "RE":
                        return AdmissionApplicationStatusTypesCategory.Rejected;
                    case "WI":
                        return AdmissionApplicationStatusTypesCategory.Withdrawn;
                    case "MS":
                        return AdmissionApplicationStatusTypesCategory.MovedToStudent;
                    default:
                        return AdmissionApplicationStatusTypesCategory.NotSet;
                }                
            }
            else
            {
                return AdmissionApplicationStatusTypesCategory.NotSet;
            }            
        }

        private SectionRegistrationStatus GetSectionRegistrationStatus(string processingCode)
        {
            switch (processingCode)
            {
                case "1":
                    return new SectionRegistrationStatus() { RegistrationStatus = RegistrationStatus.Registered, SectionRegistrationStatusReason = RegistrationStatusReason.Registered };
                case "2":
                    return new SectionRegistrationStatus() { RegistrationStatus = RegistrationStatus.Registered, SectionRegistrationStatusReason = RegistrationStatusReason.Registered };
                case "3":
                    return new SectionRegistrationStatus() { RegistrationStatus = RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = RegistrationStatusReason.Dropped };
                case "4":
                    return new SectionRegistrationStatus() { RegistrationStatus = RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = RegistrationStatusReason.Withdrawn };
                case "5":
                    return new SectionRegistrationStatus() { RegistrationStatus = RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = RegistrationStatusReason.Dropped };
                case "6":
                    return new SectionRegistrationStatus() { RegistrationStatus = RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = RegistrationStatusReason.Canceled };
                default:
                    return new SectionRegistrationStatus() { RegistrationStatus = RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = RegistrationStatusReason.Transfer };
            }
        }

        /// <summary>
        /// Section status codes
        /// </summary>
        public async Task<IEnumerable<SectionStatusCode>> GetSectionStatusCodesAsync()
        {
            return await GetValcodeAsync<SectionStatusCode>("ST", "SECTION.STATUSES",
                ss => new SectionStatusCode(ss.ValInternalCodeAssocMember, ss.ValExternalRepresentationAssocMember,
                    ConvertSectionStatusActionToStatusType(ss.ValActionCode1AssocMember),
                    ConvertSectionStatusActionToIntegrationStatusType(ss.ValActionCode3AssocMember)));
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
        /// Convert a section status action code into a section status type
        /// </summary>
        /// <param name="action">The action code of the section status</param>
        /// <returns>The section status</returns>
        public SectionStatusIntegration? ConvertSectionStatusActionToIntegrationStatusType(string action)
        {
            if (string.IsNullOrEmpty(action))
            {
                return null;
            }
            switch (action)
            {
                case "open":
                    return SectionStatusIntegration.Open;
                case "closed":
                    return SectionStatusIntegration.Closed;
                case "cancelled":
                    return SectionStatusIntegration.Cancelled;
                default:
                    return SectionStatusIntegration.Pending;
            }
        }

        /// <summary>
        /// Get a collection of SectionStatuses
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of SectionStatuses</returns>
        public async Task<IEnumerable<SectionStatuses>> GetSectionStatusesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<SectionStatuses>("ST", "SECTION.STATUSES",
                (cl, g) => new SectionStatuses(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)) 
                    { Category = ConvertSectionStatusActionToIntegrationStatusType2(cl.ValActionCode1AssocMember),
                    SectionStatusIntg = ConvertSectionStatusActionToIntegrationStatusType(cl.ValActionCode3AssocMember)
                }, bypassCache: ignoreCache);
        }

        /// <summary>
        /// Convert a section status action code into a section status type
        /// </summary>
        /// <param name="action">The action code of the section status</param>
        /// <returns>The section status</returns>
        public SectionStatusIntegration ConvertSectionStatusActionToIntegrationStatusType2(string action)
        {
            switch (action)
            {
                case "1":
                    return SectionStatusIntegration.Open;
                case "2":
                    return SectionStatusIntegration.Cancelled;
                default:
                    return SectionStatusIntegration.Pending;
            }
        }

        public async Task<IEnumerable<Specialization>> GetSpecializationsAsync()
        {
            return await GetCodeItemAsync<Specializations, Specialization>("AllSpecializations", "SPECIALIZATIONS",
                s => new Specialization(s.Recordkey, s.SpecDesc));
        }

        public async Task<IEnumerable<StudentLoad>> GetStudentLoadsAsync()
        {
            var studentLoads = await GetValcodeAsync<StudentLoad>("ST", "STUDENT.LOADS",
                sl =>
                {
                    try
                    {
                        return new StudentLoad(sl.ValInternalCodeAssocMember, sl.ValExternalRepresentationAssocMember) { Sp1 = sl.ValActionCode1AssocMember };

                    }
                    catch (Exception e)
                    {
                        // Log and return null for codes without a description.
                        LogDataError("STUDENT.LOADS", sl.ValInternalCodeAssocMember, null, e, string.Format("Failed to add student load {0}", sl.ValInternalCodeAssocMember));
                        return null;
                    }
                }
                );
            // Exclude nulls from codes without a description.
            return studentLoads.Where(sl => sl != null).ToList();
        }
        
        /// <summary>
        /// Get a collection of Student Statuses
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of student statuses</returns>
        public async Task<IEnumerable<StudentStatus>> GetStudentStatusesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<StudentStatus>("ST", "STUDENT.TERM.STATUSES",
                (ss, g) => new StudentStatus(g, ss.ValInternalCodeAssocMember, 
                ss.ValExternalRepresentationAssocMember, ss.ValActionCode1AssocMember), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for Student Statuses code
        /// </summary>
        /// <param name="code">Student Statuses code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetStudentStatusesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetStudentStatusesAsync(false);
            StudentStatus codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }
            
            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetStudentStatusesAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST.VALCODE - STUDENT.TERM.STATUSES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST.VALCODE - STUDENT.TERM.STATUSES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST.VALCODE - STUDENT.TERM.STATUSES', Record ID:'", code, "'"));
            }
            return guid;

        }

        /// <summary>
        /// Get a collection of student types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of student types</returns>
        public async Task<IEnumerable<StudentType>> GetStudentTypesAsync(bool ignoreCache = false)
        {
            return await GetGuidCodeItemAsync<StudentTypes, StudentType>("AllStudentTypes", "STUDENT.TYPES",
                (s, g) => new StudentType(g, s.Recordkey, s.SttDesc), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for Student Types code
        /// </summary>
        /// <param name="code">Student Types code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetStudentTypesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetStudentTypesAsync(false);
            StudentType codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }
            
            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetStudentTypesAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'STUDENT.TYPES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'STUDENT.TYPES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'STUDENT.TYPES', Record ID:'", code, "'"));
            }
            return guid;

        }

        public async Task<IEnumerable<Subject>> GetSubjectsAsync()
        {
            return await GetSubjectsAsync(false);
        }

        /// <summary>
        /// Get a collection of subjects
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of subjects</returns>
        public async Task<IEnumerable<Subject>> GetSubjectsAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<Subjects, Subject>("AllSubjects", "SUBJECTS",
                (s, g) =>
                {
                    return new Subject(g, s.Recordkey, s.SubjDesc, (s.SubjSelfServCourseCatlg == "Y" ? true : false)) { IntgDepartment = s.SubjIntgDept };
                }, bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for Subject code
        /// </summary>
        /// <param name="code">Subject code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetSubjectGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetSubjectsAsync(false);
            Subject codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetSubjectsAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'SUBJECTS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'SUBJECTS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'SUBJECTS', Record ID:'", code, "'"));
            }
            return guid;

        }

        /// <summary>
        /// Get a collection of Test Sources
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of test sources</returns>
        public async Task<IEnumerable<TestSource>> GetTestSourcesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<TestSource>("ST", "APPL.TEST.SOURCES",
                (es, g) => new TestSource(g, es.ValInternalCodeAssocMember, (string.IsNullOrEmpty(es.ValExternalRepresentationAssocMember)
                    ? es.ValInternalCodeAssocMember : es.ValExternalRepresentationAssocMember))
                {  actionCode1 = es.ValActionCode1AssocMember } , bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for TestSources code
        /// </summary>
        /// <param name="code">TestSources code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetTestSourcesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetTestSourcesAsync(false);
            TestSource codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetTestSourcesAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST-VALCODES, APPL.TEST.SOURCES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST-VALCODES, APPL.TEST.SOURCES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ST-VALCODES, APPL.TEST.SOURCES', Record ID:'", code, "'"));
            }
            return guid;
        }

        public async Task<IEnumerable<TopicCode>> GetTopicCodesAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<TopicCodes, TopicCode>("AllTopicCodes", "TOPIC.CODES",
                (al,g) => new TopicCode(g, al.Recordkey, al.TopcDesc), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for TopicCode code
        /// </summary>
        /// <param name="code">TopicCode code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetTopicCodeGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetTopicCodesAsync(false);
            TopicCode codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetTopicCodesAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'TOPIC.CODES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'TOPIC.CODES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'TOPIC.CODES', Record ID:'", code, "'"));
            }
            return guid;

        }


        public async Task<IEnumerable<TranscriptCategory>> GetTranscriptCategoriesAsync()
        {
            return await GetValcodeAsync<TranscriptCategory>("ST", "TRANSCRIPT.CATEGORIES",
                tc => new TranscriptCategory(tc.ValInternalCodeAssocMember, tc.ValExternalRepresentationAssocMember));
        }

        public async Task<IEnumerable<Test>> GetTestsAsync()
        {
            try
            {
                var Tests = await GetOrAddToCacheAsync<List<Test>>("AllTests",
                   async () =>
                   {
                       Collection<NonCourses> TestsData = await DataReader.BulkReadRecordAsync<NonCourses>("NON.COURSES", "WITH NCRS.CATEGORY.INDEX = 'A''P''T'");
                       var testsList = BuildTests(TestsData);
                       return testsList;
                   }
                );
                return Tests;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired fetching tests");
                throw;
            }           
        }

        public async Task<IEnumerable<WaitlistStatusCode>> GetWaitlistStatusCodesAsync()
        {
            return await GetValcodeAsync<WaitlistStatusCode>("ST", "WAIT.LIST.STATUSES",
                wl => new WaitlistStatusCode(wl.ValInternalCodeAssocMember, wl.ValExternalRepresentationAssocMember, ConvertWaitlistActionToStatus(wl.ValActionCode1AssocMember)));
        }

        public async Task<IEnumerable<SectionTransferStatus>> GetSectionTransferStatusesAsync()
        {
            return await GetValcodeAsync<SectionTransferStatus>("ST", "TRANSFER.STATUSES",
                ts => new SectionTransferStatus(ts.ValInternalCodeAssocMember, ts.ValExternalRepresentationAssocMember));
        }

        private WaitlistStatus ConvertWaitlistActionToStatus(string actionCode)
        {
            WaitlistStatus status;
            if (Enum.TryParse<WaitlistStatus>(actionCode, out status))
            {
                return status;
            }
            return WaitlistStatus.Unknown;
        }

        private List<Test> BuildTests(Collection<NonCourses> testData)
        {
            var tests = new List<Test>();
            // If no data passed in, return a null collection
            if (testData != null)
            {
                foreach (var tst in testData)
                {
                    try
                    {
                        var test = new Test(tst.Recordkey, tst.NcrsShortTitle);
                        test.MaximumScore = tst.NcrsMaxScore;
                        test.SubTestsCodes = tst.NcrsSubNcrsIds;

                        switch (tst.NcrsCategoryIdx.ToUpper())
                        {
                            case "A":
                                test.Type = TestType.Admissions;
                                break;
                            case "P":
                                test.Type = TestType.Placement;
                                break;
                            default:
                                test.Type = TestType.Other;
                                break;
                        }
                        tests.Add(test);
                    }
                    catch (Exception ex)
                    {
                        LogDataError("Test record", tst.Recordkey, tst, ex);
                        //throw new ArgumentException("Error occurred when trying to build Test " + tst.Recordkey);
                    }

                }
            }
            return tests;
        }

        private CourseStatus GetCourseStatus(string processingCode)
        {
            switch (processingCode)
            {
                case "1":
                    return CourseStatus.Active;
                case "2":
                    return CourseStatus.Terminated;
                default:
                    return CourseStatus.Unknown;
            }
        }

        private CreditType GetCreditType(string category)
        {
            switch (category)
            {
                case "I":
                    return CreditType.Institutional;
                case "C":
                    return CreditType.ContinuingEducation;
                case "T":
                    return CreditType.Transfer;
                case "E":
                    return CreditType.Exchange;
                case "O":
                    return CreditType.Other;
                default:
                    return CreditType.None;
            }
        }

        private EnrollmentStatusType GetEnrollmentStatusType(string processingCode)
        {
            switch (processingCode)
            {
                case "1":
                    return EnrollmentStatusType.inactive;
                case "2":
                    return EnrollmentStatusType.active;
                case "3":
                    return EnrollmentStatusType.complete;
                case "4":
                    return EnrollmentStatusType.inactive;
                case "5":
                    return EnrollmentStatusType.inactive;
                default:
                    return EnrollmentStatusType.inactive;
            }
        }

        private MealPlanRatePeriods GetMealPlanRatePeriods(string mealPlanRatePeriods)
        {
            switch (mealPlanRatePeriods)
            {
                case "B":
                    return MealPlanRatePeriods.Meal;
                case "D":
                    return MealPlanRatePeriods.Day;
                case "W":
                    return MealPlanRatePeriods.Week;
                case "M":
                    return MealPlanRatePeriods.Month;
                case "Y":
                    return MealPlanRatePeriods.Year;
                case "T":
                    return MealPlanRatePeriods.Term;
                default:
                    return MealPlanRatePeriods.NotSet;
            }
        }

        // TODO: Clean  up
        public async Task<IEnumerable<NoncourseStatus>> GetNoncourseStatusesAsync()
        {
            return 
                await GetValcodeAsync<NoncourseStatus>("ST", "STUDENT.NON.COURSE.STATUSES", ncStatus =>
            {
                try
                {
                    NoncourseStatusType statusType = NoncourseStatusType.None;
                    switch (ncStatus.ValActionCode1AssocMember)
                    {
                        case "1":
                            statusType = NoncourseStatusType.Withdrawn;
                            break;
                        case "2":
                            statusType = NoncourseStatusType.Accepted;
                            break;
                        case "3":
                            statusType = NoncourseStatusType.Notational;
                            break;
                        default:
                            statusType = NoncourseStatusType.None;
                            break;
                    }

                    return new NoncourseStatus(ncStatus.ValInternalCodeAssocMember, ncStatus.ValExternalRepresentationAssocMember, statusType);
                }
                catch (ColleagueSessionExpiredException csee)
                {
                    logger.Error(csee, "Colleague session expired while fetching non-course statuses");
                    throw;
                }
            });
        }

        /// <summary>
        /// Waiver reasons used by StudentWaivers
        /// </summary>
        public async Task<IEnumerable<StudentWaiverReason>> GetStudentWaiverReasonsAsync()
        {
            return await GetValcodeAsync<StudentWaiverReason>("ST", "REQUISITE.WAIVER.REASONS", reason => new StudentWaiverReason(reason.ValInternalCodeAssocMember, reason.ValExternalRepresentationAssocMember), Level1CacheTimeoutValue);
        }

        /// <summary>
        /// Petition Statuses used by Student Petitions and Faculty Consent 
        /// </summary>
        public async Task<IEnumerable<PetitionStatus>> GetPetitionStatusesAsync()
        {
            return await GetCodeItemAsync<PetitionStatuses, PetitionStatus>("AllPetitionStatuses", "PETITION.STATUSES",
                p => new PetitionStatus(p.Recordkey, p.PetDesc)
                {
                    IsGranted = (p.PetGrantedFlag.ToUpper() == "Y")
                });
        }

        /// <summary>
        /// Student Petition Reasons used by Student Petitions and Faculty Consent
        /// </summary>
        public async Task<IEnumerable<StudentPetitionReason>> GetStudentPetitionReasonsAsync()
        {
            return await GetValcodeAsync<StudentPetitionReason>("ST", "STUDENT.PETITIONS.REASON.CODES", reason => new StudentPetitionReason(reason.ValInternalCodeAssocMember, reason.ValExternalRepresentationAssocMember), Level1CacheTimeoutValue);
        }
        public async Task<IEnumerable<CapSize>> GetCapSizesAsync()
        {
            return await GetValcodeAsync<CapSize>("ST", "GRADUATION.CAP.SIZES", c => new CapSize(c.ValInternalCodeAssocMember, c.ValExternalRepresentationAssocMember));
        }

        /// <summary>
        /// Returns Student Gown Size Instance Asynchronously
        /// </summary>
        public async Task<IEnumerable<GownSize>> GetGownSizesAsync()
        {
            return await GetValcodeAsync<GownSize>("ST", "GRADUATION.GOWN.SIZES", size => new GownSize(size.ValInternalCodeAssocMember, size.ValExternalRepresentationAssocMember), Level1CacheTimeoutValue);
        }

        /// <summary>
        /// Get session cycles
        /// </summary>
        public async Task<IEnumerable<SessionCycle>> GetSessionCyclesAsync()
        {
            return await GetCodeItemAsync<SessionCycles, SessionCycle>("AllSessionCycles", "SESSION.CYCLES",
                 c => new SessionCycle(c.Recordkey, c.ScDesc));
        }

        /// <summary>
        /// Get yearly cycles
        /// </summary>
        public async Task<IEnumerable<YearlyCycle>> GetYearlyCyclesAsync()
        {
            return await GetCodeItemAsync<YearlyCycles, YearlyCycle>("AllYearlyCycles", "YEARLY.CYCLES",
                 c => new YearlyCycle(c.Recordkey, c.YcDesc));
        }

        /// <summary>
        /// Get hold request types
        /// </summary>
        public async Task<IEnumerable<HoldRequestType>> GetHoldRequestTypesAsync()
        {
            try
            {
                return await GetValcodeAsync<HoldRequestType>("ST", "STU.REQUEST.LOG.HOLDS", c => new HoldRequestType(c.ValInternalCodeAssocMember, c.ValExternalRepresentationAssocMember), Level1CacheTimeoutValue);
            }
            catch (ColleagueSessionExpiredException ce)
            {
                string message = "Colleague session got expired  while retrieving the hold request type information.";
                logger.Error(ce, message);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception occured while retrieving the hold request type information.");
                throw;
            }
        }

        /// <summary>
        /// Returns student cohorts entities
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<StudentCohort>> GetAllStudentCohortAsync(bool bypassCache)
        {
            List<StudentCohort> cohorts = new List<StudentCohort>();

            var instCohorts = await GetAllInstitutionCohortAsync(bypassCache);
            var fedCohorts = await GetAllFedCohortAsync(bypassCache);

            cohorts.AddRange(instCohorts);
            cohorts.AddRange(fedCohorts);

            return cohorts;
        }

        /// <summary>
        /// Get guid for AcademicLevels code
        /// </summary>
        /// <param name="code">AcademicLevels code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetStudentCohortGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetAllStudentCohortAsync(false);
            StudentCohort codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAllStudentCohortAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ACAD.LEVELS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ACAD.LEVELS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ACAD.LEVELS', Record ID:'", code, "'"));
            }
            return guid;

        }

        /// <summary>
        /// Returns student cohorts entities
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<StudentCohort>> GetAllInstitutionCohortAsync(bool bypassCache)
        {
            return await GetGuidValcodeAsync<StudentCohort>("ST", "INSTITUTION.COHORTS",
                (gcr, g) => new StudentCohort(g, gcr.ValInternalCodeAssocMember, gcr.ValExternalRepresentationAssocMember) { CohortType = "INSTITUTION" }, bypassCache: bypassCache);
        }

        /// <summary>
        /// Returns student cohorts entities
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<StudentCohort>> GetAllFedCohortAsync(bool bypassCache)
        {
            return await GetGuidValcodeAsync<StudentCohort>("ST", "FED.COHORTS",
                (gcr, g) => new StudentCohort(g, gcr.ValInternalCodeAssocMember, gcr.ValExternalRepresentationAssocMember) { CohortType = "FED" }, bypassCache: bypassCache);
        }

        /// <summary>
        /// Returns all student classifications entities
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<StudentClassification>> GetAllStudentClassificationAsync(bool bypassCache)
        {
            return await GetGuidCodeItemAsync<Classes, StudentClassification>("AllStudentClassifications", "CLASSES",
                (r, g) => new StudentClassification(g, r.Recordkey, r.ClsDesc), bypassCache: bypassCache);
        }

        /// <summary>
        /// Get guid for Student Classification code
        /// </summary>
        /// <param name="code">Student Classification code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetStudentClassificationGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetAllStudentClassificationAsync(false);
            StudentClassification codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }
            
            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAllStudentClassificationAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CLASSES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CLASSES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CLASSES', Record ID:'", code, "'"));
            }
            return guid;

        }


        /// <summary>
        /// Returns schedule terms entities
        /// </summary>
        /// <param name="bypassCache">Returned cached values or not</param>
        /// <returns></returns>
        public async Task<IEnumerable<ScheduleTerm>> GetAllScheduleTermsAsync(bool bypassCache)
        {
            return await GetGuidValcodeAsync<ScheduleTerm>("ST", "SCHEDULE.TERMS",
                (gcr, g) => new ScheduleTerm(g, gcr.ValInternalCodeAssocMember, gcr.ValExternalRepresentationAssocMember), bypassCache: bypassCache);
        }

        /// <summary>
        /// Get a collection of BillingOverrideReasons
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of BillingOverrideReasons</returns>
        public async Task<IEnumerable<BillingOverrideReasons>> GetBillingOverrideReasonsAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<BillingOverrideReasons>("ST", "BILLING.OVERRIDE.REASONS",
                (e, g) => new BillingOverrideReasons(g, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Returns all withdraw reasons entities
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<WithdrawReason>> GetWithdrawReasonsAsync(bool bypassCache)
        {
            return await GetGuidCodeItemAsync<WithdrawReasons, WithdrawReason>("AllWithdrawReasons", "WITHDRAW.REASONS",
                (r, g) => new WithdrawReason(g, r.Recordkey, r.WdrDesc), bypassCache: bypassCache);
        }

        /// <summary>
        /// Get guid for WithdrawReasons code
        /// </summary>
        /// <param name="code">WithdrawReasons code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetWithdrawReasonsGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetWithdrawReasonsAsync(false);
            WithdrawReason codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetWithdrawReasonsAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'WITHDRAW.REASONS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'WITHDRAW.REASONS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'WITHDRAW.REASONS', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// Get a collection of FloorPreferences
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of FloorPreferences</returns>
        public async Task<IEnumerable<FloorCharacteristics>> GetFloorCharacteristicsAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<FloorCharacteristics>("ST", "FLOOR.PREFERENCES",
                 (cl, g) => new FloorCharacteristics(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                     ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of MealClass
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of MealClass</returns>
        public async Task<IEnumerable<StudentResidentialCategories>> GetStudentResidentialCategoriesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<StudentResidentialCategories>("ST", "MEAL.CLASS",
                (m, g) => new StudentResidentialCategories(g, m.ValInternalCodeAssocMember, m.ValExternalRepresentationAssocMember), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of RoommateCharacteristics
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of RoommateCharacteristics</returns>
        public async Task<IEnumerable<RoommateCharacteristics>> GetRoommateCharacteristicsAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<RoommateCharacteristics>("ST", "ROOMMATE.CHARACTERISTICS",
                (cl, g) => new RoommateCharacteristics(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }


        /// <summary>
        /// Get a collection of AttendanceTypes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of AttendanceTypes</returns>
        public async Task<IEnumerable<AttendanceTypes>> GetAttendanceTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<AttendanceTypes>("ST", "ATTENDANCE.TYPES",
                (cl, g) => new AttendanceTypes(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Returns Contact Measures valcode table Asynchronously (ContactHoursPeriod in service layer)
        /// </summary>
        public async Task<IEnumerable<ContactMeasure>> GetContactMeasuresAsync(bool ignoreCache)
        {
            return await GetValcodeAsync("ST", "CONTACT.MEASURES",  
                cm => new ContactMeasure(cm.ValInternalCodeAssocMember, cm.ValExternalRepresentationAssocMember, 
                cm.ValActionCode3AssocMember), timeout: Level1CacheTimeoutValue, bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of financial aid academic progress types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid fund classifications</returns>
        public async Task<IEnumerable<Domain.Student.Entities.SapType>> GetSapTypesAsync(bool ignoreCache = false)
        {
            return await GetGuidCodeItemAsync<DataContracts.SapType, Domain.Student.Entities.SapType>("AllFinancialAidAcademicProgressTypes", "SAP.TYPE",
                (st, g) => new Domain.Student.Entities.SapType(g, st.Recordkey, String.IsNullOrWhiteSpace(st.SaptDesc) ? st.Recordkey : st.SaptDesc), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of SapStatuses
        /// </summary>
        /// <param name="restrictedVisibilityValue">Filter namedQuery</param>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of SapStatuses</returns>
        public async Task<IEnumerable<SapStatuses>> GetSapStatusesAsync(string restrictedVisibilityValue = "", bool ignoreCache = false)
        {
            var faSapStatusInfoData = new Collection<FaSapStatusInfo>();//
            if (!string.IsNullOrEmpty(restrictedVisibilityValue))
            {
                faSapStatusInfoData = await DataReader.BulkReadRecordAsync<FaSapStatusInfo>(string.Format("WITH FSSI.CATEGORY EQ '{0}'", restrictedVisibilityValue));
                if (faSapStatusInfoData == null || (faSapStatusInfoData != null && !faSapStatusInfoData.Any()))
                {
                    return new List<SapStatuses>();
                }
                string[] fssiStatuses = faSapStatusInfoData.Select(i => i.FssiStatus).ToArray();
                var results = await GetGuidValcodeAsync("ST", "SAP.STATUSES", (cl, g) => new SapStatuses(g, cl.ValInternalCodeAssocMember,
                              (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember) ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)),
                              bypassCache: ignoreCache);
                if (results == null || !results.Any())
                {
                    return new List<SapStatuses>();
                }
                return results.Where(i => fssiStatuses.Contains(i.Code));
            }
            return await GetGuidValcodeAsync("ST", "SAP.STATUSES", (cl, g) => new SapStatuses(g, cl.ValInternalCodeAssocMember,
                (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember) ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Read the international parameters records to extract date format used
        /// locally and setup in the INTL parameters.
        /// </summary>
        /// <returns>International Parameters with date properties</returns>
        private async Task<Ellucian.Colleague.Data.Base.DataContracts.IntlParams> GetInternationalParametersAsync()
        {
            if (internationalParameters != null)
            {
                return internationalParameters;
            }
            // Overriding cache timeout to be Level1 Cache time out for data that rarely changes.
            internationalParameters = await GetOrAddToCacheAsync<Ellucian.Colleague.Data.Base.DataContracts.IntlParams>("InternationalParameters",
                async () =>
                {
                    Data.Base.DataContracts.IntlParams intlParams = await DataReader.ReadRecordAsync<Data.Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL");
                    if (intlParams == null)
                    {
                        var errorMessage = "Unable to access international parameters INTL.PARAMS INTERNATIONAL.";
                        logger.Info(errorMessage);
                        // If we cannot read the international parameters default to US with a / delimiter.
                        // throw new ColleagueWebApiException(errorMessage);
                        Data.Base.DataContracts.IntlParams newIntlParams = new Data.Base.DataContracts.IntlParams();
                        newIntlParams.HostShortDateFormat = "MDY";
                        newIntlParams.HostDateDelimiter = "/";
                        newIntlParams.HostCountry = "USA";
                        intlParams = newIntlParams;
                    }
                    return intlParams;
                }, Level1CacheTimeoutValue);
            return internationalParameters;
        }

        public async Task<string> GetHostCountryAsync()
        {
            var intlParams = await GetInternationalParametersAsync();
            return intlParams.HostCountry;
        }

        /// <summary>
        /// Get the GuidLookupResult for a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>GuidLookupResult or KeyNotFoundException if supplied Guid was not found</returns>
        public async Task<GuidLookupResult> GetGuidLookupResultFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || !idDict.Any())
            {
                throw new KeyNotFoundException("GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();

            if (!string.IsNullOrEmpty(foundEntry.Key) && foundEntry.Value != null)
            {
                return foundEntry.Value;
            }
            else
            {
                throw new KeyNotFoundException("GUID " + guid + " not found.");
            }
        }

        public async Task<IEnumerable<DropReason>> GetDropReasonsAsync()
        {
            return await GetValcodeAsync<DropReason>("ST", "STUDENT.ACAD.CRED.STATUS.REASONS", reason => new DropReason(reason.ValInternalCodeAssocMember, reason.ValExternalRepresentationAssocMember, reason.ValActionCode1AssocMember), Level1CacheTimeoutValue);
        }


        /// <summary>
        /// Get a collection of SectionTitleType
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of SectionTitleType</returns>
        public async Task<IEnumerable<SectionTitleType>> GetSectionTitleTypesAsync(bool ignoreCache)
        {

            {
                return await GetGuidCodeItemAsync<IntgSecTitleTypes, SectionTitleType>("AllSectionTitleTypes", "INTG.SEC.TITLE.TYPES",
                    (c, g) => new SectionTitleType(g, c.Recordkey, c.IsttDesc) { Title = c.IsttTitle }, bypassCache: ignoreCache);
            }
        }

        /// <summary>
        /// Get guid for WithdrawReasons code
        /// </summary>
        /// <param name="code">WithdrawReasons code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetSectionTitleTypesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetSectionTitleTypesAsync(false);
            SectionTitleType codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.ToUpper().Equals(code.ToUpper(), StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetSectionTitleTypesAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'INTG.SEC.TITLE.TYPES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.ToUpper().Equals(code.ToUpper(), StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'INTG.SEC.TITLE.TYPES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'INTG.SEC.TITLE.TYPES', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// Get a collection of SectionDescriptionType
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of SectionDescriptionType</returns>
        public async Task<IEnumerable<SectionDescriptionType>> GetSectionDescriptionTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<SectionDescriptionType>("ST", "INTG.SEC.DESC.TYPES",
                (sdt, g) => new SectionDescriptionType(g, sdt.ValInternalCodeAssocMember, (string.IsNullOrEmpty(sdt.ValExternalRepresentationAssocMember)
                    ? sdt.ValInternalCodeAssocMember : sdt.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Gets a GUID for SectionDescriptionType.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<string> GetSectionDescriptionTypesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetSectionDescriptionTypesAsync(false);
            SectionDescriptionType codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.ToUpper().Equals(code.ToUpper(), StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetSectionDescriptionTypesAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'INTG.SEC.DESC.TYPES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.ToUpper().Equals(code.ToUpper(), StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'INTG.SEC.DESC.TYPES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'INTG.SEC.DESC.TYPES', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// Get a collection of grading terms.
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid award periods</returns>
        public async Task<IEnumerable<GradingTerm>> GetGradingTermsAsync(bool bypassCache = false)
        {
            return await GetValcodeAsync<GradingTerm>("ST", "GRADING.TERMS", r =>
                (new GradingTerm(r.ValInternalCodeAssocMember, r.ValExternalRepresentationAssocMember)), bypassCache: bypassCache);
        }


        /// <summary>
        /// Get a collection of financial aid fund categories
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid fund categories</returns>
        public async Task<IEnumerable<FinancialAidFundCategory>> GetFinancialAidFundCategoriesAsync(bool ignoreCache = false)
        {
            return await GetGuidCodeItemAsync<AwardCategories, FinancialAidFundCategory>("AllFinancialAidFundCategories", "AWARD.CATEGORIES",
                (fa, g) =>
                {
                    AwardCategoryType? type = null;
                    var typeArray = new string[4] { fa.AcLoanFlag, fa.AcGrantFlag, fa.AcScholarshipFlag, fa.AcWorkFlag };

                    //is exactly one of the flags equal to Yes?
                    if (typeArray.Where(t => !string.IsNullOrEmpty(t) && t.ToUpper() == "Y").Count() == 1)
                    {
                        if (!string.IsNullOrEmpty(fa.AcLoanFlag) && fa.AcLoanFlag.ToUpper() == "Y") type = AwardCategoryType.Loan;
                        else if (!string.IsNullOrEmpty(fa.AcGrantFlag) && fa.AcGrantFlag.ToUpper() == "Y") type = AwardCategoryType.Grant;
                        else if (!string.IsNullOrEmpty(fa.AcScholarshipFlag) && fa.AcScholarshipFlag.ToUpper() == "Y") type = AwardCategoryType.Scholarship;
                        else if (!string.IsNullOrEmpty(fa.AcWorkFlag) && fa.AcWorkFlag.ToUpper() == "Y") type = AwardCategoryType.Work;
                    }

                    var restrictedFlag = fa.AcIntgRestricted.ToUpper() == "Y" ? true : false;
                    var categoryName = ConvertCategoryName(fa.AcIntgName);

                    return new FinancialAidFundCategory(g, fa.Recordkey, String.IsNullOrEmpty(fa.AcDescription) ? fa.Recordkey : fa.AcDescription, type, categoryName, restrictedFlag);
                }, bypassCache: ignoreCache);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a FinancialAidFundsSource domain enumeration value to its corresponding FinancialAidFundsSource DTO enumeration value
        /// </summary>
        /// <param name="source">FinancialAidFundsSource domain enumeration value</param>
        /// <returns>FinancialAidFundsSource DTO enumeration value</returns>
        private FinancialAidFundAidCategoryType ConvertCategoryName(string source)
        {
            switch (source)
            {

                case "1":
                    return FinancialAidFundAidCategoryType.PellGrant;
                case "2":
                    return FinancialAidFundAidCategoryType.FederalUnsubsidizedLoan;
                case "3":
                    return FinancialAidFundAidCategoryType.FederalSubsidizedLoan;
                case "4":
                    return FinancialAidFundAidCategoryType.GraduateTeachingGrant;
                case "5":
                    return FinancialAidFundAidCategoryType.UndergraduateTeachingGrant;
                case "6":
                    return FinancialAidFundAidCategoryType.ParentPlusLoan;
                case "7":
                    return FinancialAidFundAidCategoryType.GraduatePlusLoan;
                case "8":
                    return FinancialAidFundAidCategoryType.FederalWorkStudyProgram;
                case "9":
                    return FinancialAidFundAidCategoryType.IraqAfghanistanServiceGrant;
                case "10":
                    return FinancialAidFundAidCategoryType.AcademicCompetitivenessGrant;
                case "11":
                    return FinancialAidFundAidCategoryType.BureauOfIndianAffairsFederalGrant;
                case "12":
                    return FinancialAidFundAidCategoryType.RobertCByrdScholarshipProgram;
                case "13":
                    return FinancialAidFundAidCategoryType.PaulDouglasTeacherScholarship;
                case "14":
                    return FinancialAidFundAidCategoryType.GeneralTitleIVloan;
                case "15":
                    return FinancialAidFundAidCategoryType.HealthEducationAssistanceLoan;
                case "16":
                    return FinancialAidFundAidCategoryType.HealthProfessionalStudentLoan;
                case "17":
                    return FinancialAidFundAidCategoryType.IncomeContingentLoan;
                case "18":
                    return FinancialAidFundAidCategoryType.LoanForDisadvantagesStudent;
                case "19":
                    return FinancialAidFundAidCategoryType.LeveragingEducationalAssistancePartnership;
                case "20":
                    return FinancialAidFundAidCategoryType.NationalHealthServicesCorpsScholarship;
                case "21":
                    return FinancialAidFundAidCategoryType.NursingStudentLoan;
                case "22":
                    return FinancialAidFundAidCategoryType.PrimaryCareLoan;
                case "23":
                    return FinancialAidFundAidCategoryType.RotcScholarship;
                case "24":
                    return FinancialAidFundAidCategoryType.FederalSupplementaryEducationalOpportunityGrant;
                case "25":
                    return FinancialAidFundAidCategoryType.StayInSchoolProgram;
                case "26":
                    return FinancialAidFundAidCategoryType.FederalSupplementaryLoanForParent;
                case "27":
                    return FinancialAidFundAidCategoryType.NationalSmartGrant;
                case "28":
                    return FinancialAidFundAidCategoryType.StateStudentIncentiveGrant;
                case "29":
                    return FinancialAidFundAidCategoryType.VaHealthProfessionsScholarship;
                case "30":
                    return FinancialAidFundAidCategoryType.NonGovernmental;
                case "31":
                    return FinancialAidFundAidCategoryType.FederalPerkinsLoan;
                default:
                    return FinancialAidFundAidCategoryType.NonGovernmental;
            }
        }


        /// <summary>
        /// Get a collection of financial aid fund classifications
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid fund classifications</returns>
        public async Task<IEnumerable<FinancialAidFundClassification>> GetFinancialAidFundClassificationsAsync(bool ignoreCache = false)
        {
            return await GetGuidCodeItemAsync<ReportFundTypes, FinancialAidFundClassification>("AllFinancialAidFundClassifications", "REPORT.FUND.TYPES",
                (fa, g) => new FinancialAidFundClassification(g, fa.RftFundTypeCode, String.IsNullOrEmpty(fa.RftTitle) ? fa.RftFundTypeCode : fa.RftTitle)
                { Description2 = fa.RftDesc, FundingTypeCode = fa.Recordkey }, bypassCache: ignoreCache);
        }

        /// <summary>
        /// Return a Unidata Formatted Date string from an input argument of string type
        /// </summary>
        /// <param name="date">String representing a Date</param>
        /// <returns>Unidata formatted Date string for use in Colleague Selection.</returns>
        public async Task<string> GetUnidataFormattedDate(string date)
        {
            var internationalParameters = await GetInternationalParametersAsync();
            var newDate = DateTime.Parse(date).Date;
            return UniDataFormatter.UnidataFormatDate(newDate, internationalParameters.HostShortDateFormat, internationalParameters.HostDateDelimiter);
        }

        /// <summary>
        /// Get a collection of financial aid years.
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid years</returns>
        public async Task<IEnumerable<FinancialAidYear>> GetFinancialAidYearsAsync(bool ignoreCache = false)
        {
            var hostCountry = string.Empty;
            try
            {
                hostCountry = await GetHostCountryAsync();
            }
            catch
            {
                hostCountry = string.Empty;
            }
            return await GetGuidCodeItemAsync<FaSuites, FinancialAidYear>("AllFinancialAidYears", "FA.SUITES",
                (fa, g) => new FinancialAidYear(g, fa.Recordkey, fa.Recordkey, fa.FaSuitesStatus) { HostCountry = hostCountry }, bypassCache: ignoreCache);
        }


        /// <summary>
        /// Get guid for GetFinancialAidYears code
        /// </summary>
        /// <param name="code">FinancialAidYears code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetFinancialAidYearsGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetFinancialAidYearsAsync(false);
            FinancialAidYear codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (allCodesCache == null)
            {
                var allCodesNoCache = await GetFinancialAidYearsAsync(true);
                if ( allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'FA.SUITES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'FA.SUITES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'FA.SUITES', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// Get FinancialAidYear for guid.
        /// </summary>
        /// <param name="sourceGuid"></param>
        /// <returns></returns>
        public async Task<FinancialAidYear> GetFinancialAidYearAsync( string sourceGuid )
        {
            //get all the codes from the cache
            FinancialAidYear codeCache = null;
            if( string.IsNullOrEmpty( sourceGuid ) )
                return codeCache;
            var allCodesCache = await GetFinancialAidYearsAsync( false );
            if( allCodesCache != null && allCodesCache.Any() )
            {
                codeCache = allCodesCache.FirstOrDefault( c => c.Guid.Equals( sourceGuid, StringComparison.OrdinalIgnoreCase ) );
            }

            if(codeCache == null)
            {
                throw new RepositoryException( string.Format( "Financial aid year not found., Entity:'FA.SUITES', Record GUID:'{0}'", sourceGuid ) );
            }
            //if we cannot find that code in the cache, then refresh the cache and try again.
            if( allCodesCache == null )
            {
                var allCodesNoCache = await GetFinancialAidYearsAsync( true );
                if( allCodesNoCache == null )
                {
                    throw new RepositoryException( string.Concat( "Financial aid years not found., Entity:'FA.SUITES', Record GUID:'", sourceGuid, "'" ) );
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault( c => c.Guid.Equals( sourceGuid, StringComparison.OrdinalIgnoreCase ) );
                if( codeNoCache == null )
                {
                    throw new RepositoryException( string.Concat( "Financial aid year not found., Entity:'FA.SUITES', Record GUID:'", sourceGuid, "'" ) );
                }
                else
                {
                    codeCache = codeNoCache;
                }
            }
            return codeCache;
        }

        /// <summary>
        /// Get a collection of financial aid award periods
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid award periods</returns>
        public async Task<IEnumerable<FinancialAidAwardPeriod>> GetFinancialAidAwardPeriodsAsync(bool ignoreCache = false)
        {
            return await GetGuidCodeItemAsync<AwardPeriods, FinancialAidAwardPeriod>("AllFinancialAidAwardPeriods", "AWARD.PERIODS",
                (fa, g) => new FinancialAidAwardPeriod(g, fa.Recordkey, fa.AwdpDesc, "active") { StartDate = fa.AwdpStartDate, EndDate = fa.AwdpEndDate, AwardTerms = fa.AwdpAcadTerms }, bypassCache: ignoreCache);
        }

        /// <summary>
        /// Public Accessor for Financial Aid AwardStatuses. Retrieves and caches all award statuses 
        /// defined in Colleague.
        /// Each status category in Colleague maps to one of three categories in the API. There are 5:
        /// Colleague Accepted = Accepted
        /// Colleague Pending = Pending
        /// Colleague Estimated = Pending
        /// Colleague Rejected = Rejected
        /// Colleague Denied = Rejected
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>IEnumerable<AwardStatus></returns>
        public async Task<IEnumerable<AwardStatus>> AwardStatusesAsync(bool bypassCache = false)
        {
            return await GetCodeItemAsync<AwardActions, AwardStatus>("AllAwardStatuses", "AWARD.ACTIONS",
                a =>
                {
                    AwardStatusCategory cat;
                    switch (a.AaCategory)
                    {
                        case "A":
                            cat = AwardStatusCategory.Accepted;
                            break;
                        case "P":
                            cat = AwardStatusCategory.Pending;
                            break;
                        case "E":
                            cat = AwardStatusCategory.Estimated;
                            break;
                        case "R":
                            cat = AwardStatusCategory.Rejected;
                            break;
                        case "D":
                            cat = AwardStatusCategory.Denied;
                            break;
                        default:
                            cat = AwardStatusCategory.Rejected;
                            break;
                    }

                    return new AwardStatus(a.Recordkey, a.AaDescription, cat);
                }, bypassCache: bypassCache);

        }

        /// <summary>
        /// Returns all education goals
        /// </summary>
        /// <param name="ignoreCache">Flag indicating whether or not to bypass the cache</param>
        /// <returns>Collection of <see cref="EducationGoal">education goals</see></returns>
        public async Task<IEnumerable<EducationGoal>> GetAllEducationGoalsAsync(bool bypassCache = false)
        {
            return await GetValcodeAsync<EducationGoal>("ST", "EDUCATION.GOALS", r =>
                (new EducationGoal(r.ValInternalCodeAssocMember, r.ValExternalRepresentationAssocMember)), bypassCache: bypassCache);
        }

        /// <summary>
        /// Returns all registration reasons
        /// </summary>
        /// <param name="ignoreCache">Flag indicating whether or not to bypass the cache</param>
        /// <returns>Collection of <see cref="RegistrationReason">registration reasons</see></returns>
        public async Task<IEnumerable<RegistrationReason>> GetRegistrationReasonsAsync(bool bypassCache = false)
        {
            return await GetValcodeAsync<RegistrationReason>("ST", "REG.REASONS", r =>
                (new RegistrationReason(r.ValInternalCodeAssocMember, r.ValExternalRepresentationAssocMember)), bypassCache: bypassCache);
        }

        /// <summary>
        /// Returns all registration marketing sources
        /// </summary>
        /// <param name="ignoreCache">Flag indicating whether or not to bypass the cache</param>
        /// <returns>Collection of <see cref="RegistrationMarketingSource">registration marketing sources</see></returns>
        public async Task<IEnumerable<RegistrationMarketingSource>> GetRegistrationMarketingSourcesAsync(bool bypassCache = false)
        {
            return await GetValcodeAsync<RegistrationMarketingSource>("ST", "REG.MARKETING.SOURCES", r =>
                (new RegistrationMarketingSource(r.ValInternalCodeAssocMember, r.ValExternalRepresentationAssocMember)), bypassCache: bypassCache);
        }

        /// <summary>
        /// Returns all Financial Aid Marital Statuses.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <param name="years">
        /// If year is 2018 or 2019 then years value will be 18 or if year is between 2014-2017 then value will be 14 etc.
        /// </param>
        /// <returns>IEnumerable<FinancialAidMaritalStatus></returns>
        public async Task<IEnumerable<FinancialAidMaritalStatus>> GetFinancialAidMaritalStatusesAsync( bool bypassCache = false, params string[] years )
        {
            /*
                Notes from specs
                Choose FA.STU.MARITAL.STATUS.20 for (import year 2020+)
                Choose FA.STU.MARITAL.STATUS.18 for (import year s 2018, 2019)
                Choose FA.STU.MARITAL.STATUS.14 for (import years 2014, 2015, 2016, 2017)
                Choose FA.STU.MARITAL.STATUS.10 for (import years 2010, 2011, 2012, 2013) 
            */
            List<FinancialAidMaritalStatus> finAidMStatuses = new List<FinancialAidMaritalStatus>();
            if( years == null || !years.Any() )
            {
                finAidMStatuses.AddRange( await GetValcodeAsync<FinancialAidMaritalStatus>( "ST", "FA.STU.MARITAL.STATUS.10", r =>
                         ( new FinancialAidMaritalStatus( r.ValInternalCodeAssocMember, r.ValExternalRepresentationAssocMember ) { FinancialAidYear = "10" } ), bypassCache: bypassCache ) );
            }
            else
            {
                foreach( var year in years )
                {
                    string yr = string.IsNullOrWhiteSpace( year ) ? "10" : year;
                    string finAidMaritalStatusValCode = string.Format( "FA.STU.MARITAL.STATUS.{0}", yr );
                    finAidMStatuses.AddRange( await GetValcodeAsync<FinancialAidMaritalStatus>( "ST", finAidMaritalStatusValCode, r =>
                         ( new FinancialAidMaritalStatus( r.ValInternalCodeAssocMember, r.ValExternalRepresentationAssocMember ) { FinancialAidYear = yr } ), bypassCache: bypassCache ) );
                }
            }
            return finAidMStatuses;
        }

        /// <summary>
        /// Returns a Financial Aid Marital Status.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<FinancialAidMaritalStatus> GetFinancialAidMaritalStatusAsync( string year, string code )
        {
            FinancialAidMaritalStatus codeCache = null;
            if( string.IsNullOrEmpty( year ) )
                return codeCache;
            var allCodesCache = await GetFinancialAidMaritalStatusesAsync( false, year );
            if( allCodesCache != null && allCodesCache.Any() )
            {
                codeCache = allCodesCache.FirstOrDefault( c => c.FinancialAidYear.Equals( year, StringComparison.OrdinalIgnoreCase ) && !string.IsNullOrWhiteSpace(code) && c.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase) );
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if( codeCache == null )
            {
                var allCodesNoCache = await GetFinancialAidMaritalStatusesAsync( true, year );
                if( allCodesNoCache == null )
                {
                    throw new RepositoryException( string.Concat( "No record found, Entity:'FA.STU.MARITAL.STATUS', Record ID:'", year, "'" ) );
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault( c => c.FinancialAidYear.Equals( year, StringComparison.OrdinalIgnoreCase ) && !string.IsNullOrWhiteSpace( code ) && c.Code.Equals( code, StringComparison.InvariantCultureIgnoreCase ) );
                if( codeNoCache != null )
                    return codeNoCache;
                else
                    throw new RepositoryException( string.Concat( "No record found, Entity:'FA.STU.MARITAL.STATUS', Record ID:'", year, "'" ) );
            }
            return codeCache;
        }

        /// <summary>
        /// Returns all valid intent to withdraw codes.
        /// </summary>
        /// <param name="bypassCache">Flag indicating whether or not to bypass the cache</param>
        /// <returns>All valid <see cref="IntentToWithdrawCode">intent to withdraw codes</see></returns>
        public async Task<IEnumerable<IntentToWithdrawCode>> GetIntentToWithdrawCodesAsync(bool bypassCache = false)
        {
            if (bypassCache)
            {
                var intentToWithdrawCodes = await RetrieveAndBuildIntentToWithdrawCodes();
                return intentToWithdrawCodes;
            } 
            else
            {
                return await GetOrAddToCacheAsync<IEnumerable<IntentToWithdrawCode>>("AllIntentToWithdrawCodes", async () => 
                    await RetrieveAndBuildIntentToWithdrawCodes(), 
                    Level1CacheTimeoutValue);
            }
        }

        /// <summary>
        /// Retrieves necessary data and builds <see cref="IntentToWithdrawCode"/> objects
        /// </summary>
        /// <returns>Collection of valid <see cref="IntentToWithdrawCode">intent to withdraw codes</see></returns>
        private async Task<IEnumerable<IntentToWithdrawCode>> RetrieveAndBuildIntentToWithdrawCodes()
        {
            List<IntentToWithdrawCode> intentToWithdrawCodes = new List<IntentToWithdrawCode>();
            string[] intToWdrlCodeIds = await DataReader.SelectAsync("INT.TO.WDRL.CODES", string.Empty);

            List<IntToWdrlCodes> intToWdrlCodes = new List<IntToWdrlCodes>();
            intToWdrlCodes = await BulkReadRecordWithLoggingAsync<IntToWdrlCodes>("INT.TO.WDRL.CODES", intToWdrlCodeIds.ToArray(), readSize, true, true);
            foreach(var intw in intToWdrlCodes)
            {
                try 
                {
                    var intToWdrlCode = new IntentToWithdrawCode(intw.Recordkey, intw.ItwcCode, intw.ItwcDescription);
                    intentToWithdrawCodes.Add(intToWdrlCode);
                }
                catch (Exception ex)
                {
                    LogDataError("INT.TO.WDRL.CODES", intw.Recordkey, intw, ex);
                    continue;
                }
            }
            return intentToWithdrawCodes;
        }

        /// <summary>
        /// Get student release access codes
        /// </summary>
        public async Task<IEnumerable<StudentReleaseAccess>> GetStudentReleaseAccessCodesAsync()
        {
            return await GetCodeItemAsync<StuReleaseAccess, StudentReleaseAccess>("StudentReleaseAccessRecords", "STU.RELEASE.ACCESS",
                 sr => new StudentReleaseAccess(sr.Recordkey, sr.SraDesc, sr.SraComments));
        }
    }
}