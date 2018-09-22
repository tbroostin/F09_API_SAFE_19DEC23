//Copyright 2016-2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class HumanResourcesReferenceDataRepository : BaseColleagueRepository, IHumanResourcesReferenceDataRepository
    {
        
        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public HumanResourcesReferenceDataRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Get a collection of Bargaining Units
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of bargaining units</returns>
        public async Task<IEnumerable<BargainingUnit>> GetBargainingUnitsAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<BargUnits, BargainingUnit>("AllBargainingUnits", "BARG.UNITS",
              (b, g) => new BargainingUnit(g, b.Recordkey, b.BgnDesc), CacheTimeout, this.DataReader.IsAnonymous, ignoreCache);
        }

        /// <summary>
        /// Get a collection of BeneficiaryTypes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of BeneficiaryTypes</returns>
        public async Task<IEnumerable<BeneficiaryTypes>> GetBeneficiaryTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<BeneficiaryTypes>("HR", "BENEFICIARY.TYPES",
                (cl, g) => new BeneficiaryTypes(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Gets an unfiltered list of benefit and deduction records mapped to domain entities
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<BenefitDeductionType>> GetBenefitDeductionTypesAsync()
        {
            return await GetOrAddToCacheAsync("AllBenefitDeductionTypes", async () => await BuildBenefitDeductionTypesAsync(), Level1CacheTimeoutValue);
        }

        private async Task<IEnumerable<BenefitDeductionType>> BuildBenefitDeductionTypesAsync() 
        {
            var entities = new List<BenefitDeductionType>();
            var records = await DataReader.BulkReadRecordAsync<Bended>(string.Empty);
            if (records == null || !records.Any())
            {
                logger.Info("Null Bended records returned by bulk record read");
                return entities;
            }
            foreach (var bended in records)
            {
                try
                {
                    entities.Add(
                        new BenefitDeductionType(
                            bended.Recordkey,
                            bended.BdDesc,
                            bended.BdSelfServiceDesc,
                            await TranslateInstitutionType(bended.BdInstitutionType)
                        )
                    );
                }
                catch (Exception e)
                {
                    LogDataError("BENDED", bended.Recordkey, bended, e);
                }
            }
            return entities;
        }



        /// <summary>
        /// Translate the institution type (aka bended type) into enum...
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private async Task<BenefitDeductionTypeCategory> TranslateInstitutionType(string type)
        {
            if (type != null)
            {
                var codeItem = (await GetBendedTypesValcodeAsync()).ValsEntityAssociation.FirstOrDefault(v => v.ValInternalCodeAssocMember == type);
                if (codeItem == null)
                {
                    return BenefitDeductionTypeCategory.Benefit;
                }
                switch (codeItem.ValActionCode2AssocMember.ToUpperInvariant())
                {
                    case "D": return BenefitDeductionTypeCategory.Deduction;
                    case "B": return BenefitDeductionTypeCategory.Benefit;
                    default: return BenefitDeductionTypeCategory.Benefit;
                }
            }
            else
            {
                return BenefitDeductionTypeCategory.Benefit;
            }
        }

        /// <summary>
        /// Get the bended.types from HR.VALCODES
        /// </summary>
        /// <returns></returns>
        private async Task<ApplValcodes> GetBendedTypesValcodeAsync()
        {
            var bendedTypes = await GetOrAddToCacheAsync<ApplValcodes>("BendedTypes",
               async () =>
               {
                   ApplValcodes bendedTypesValCodes = await DataReader.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "BENDED.TYPES");

                   if (bendedTypesValCodes == null)
                   {
                       var errorMessage = "Unable to access BENDED.TYPES valcode table.";
                       logger.Info(errorMessage);
                       throw new ApplicationException(errorMessage);
                   }
                   return bendedTypesValCodes;
               }, Level1CacheTimeoutValue);
            return bendedTypes;
        }

        /// <summary>
        /// Get a collection of CostCalculationMethod
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of CostCalculationMethod</returns>
        public async Task<IEnumerable<CostCalculationMethod>> GetCostCalculationMethodsAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<CostCalculationMethod>("HR", "BD.CALC.METHODS",
                (cl, g) => new CostCalculationMethod(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of DeductionCategory
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of DeductionCategory</returns>
        public async Task<IEnumerable<DeductionCategory>> GetDeductionCategoriesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<DeductionCategory>("HR", "BENDED.TYPES",
                (cl, g) => new DeductionCategory(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get all Deduction type objects, built from database data
        /// </summary>
        /// <returns>A list of Deduction type objects</returns>
        public async Task<IEnumerable<DeductionType>> GetDeductionTypesAsync(bool ignoreCache = false)
        {
            if (ignoreCache)
            {
                return await BuildAllDeductionTypes();
            }
            else
            {
                return await GetOrAddToCacheAsync<IEnumerable<DeductionType>>("AllDeductionTypes", async () => await this.BuildAllDeductionTypes(), Level1CacheTimeoutValue);
            }
        }

        private async Task<IEnumerable<DeductionType>> BuildAllDeductionTypes()
        {
            var deductionTypeEntities = new List<DeductionType>();
            var deductionTypeIds = await DataReader.SelectAsync("BENDED", "WITH BD.PAYERS = 'E''S'");

            var deductionTypeRecords = await DataReader.BulkReadRecordAsync<DataContracts.Bended>(deductionTypeIds);


            foreach (var deductionTypeRecord in deductionTypeRecords)
            {

                deductionTypeEntities.Add(new DeductionType(deductionTypeRecord.RecordGuid, deductionTypeRecord.Recordkey, deductionTypeRecord.BdDesc));

            }


            return deductionTypeEntities;
        }

        /// <summary>
        /// Get all Deduction type objects, built from database data
        /// </summary>
        /// <returns>A list of Deduction type objects</returns>
        public async Task<IEnumerable<DeductionType>> GetDeductionTypes2Async(bool ignoreCache = false)
        {
            if (ignoreCache)
            {
                return await BuildAllDeductionTypes2();
            }
            else
            {
                return await GetOrAddToCacheAsync<IEnumerable<DeductionType>>("AllDeductionTypes", async () => await this.BuildAllDeductionTypes2(), Level1CacheTimeoutValue);
            }
        }

        private async Task<IEnumerable<DeductionType>> BuildAllDeductionTypes2()
        {
            var deductionTypeEntities = new List<DeductionType>();
            var deductionTypeIds = await DataReader.SelectAsync("BENDED", "WITH BD.PAYERS = 'E''S'");

            var deductionTypeRecords = await DataReader.BulkReadRecordAsync<DataContracts.Bended>(deductionTypeIds);


            foreach (var deductionTypeRecord in deductionTypeRecords)
            {

                deductionTypeEntities.Add(new DeductionType(deductionTypeRecord.RecordGuid, deductionTypeRecord.Recordkey, string.IsNullOrEmpty(deductionTypeRecord.BdDesc) ? deductionTypeRecord.Recordkey : deductionTypeRecord.BdDesc, deductionTypeRecord.BdInstitutionType, deductionTypeRecord.BdCalcMethod, deductionTypeRecord.BdWithholdingPayCycles, deductionTypeRecord.BdTxablTaxCodes, deductionTypeRecord.BdDeferTaxCodes));

            }


            return deductionTypeEntities;
        }

        ///// <summary>
        ///// Get a collection of EmploymentFrequency
        ///// </summary>
        ///// <param name="ignoreCache">Bypass cache flag</param>
        ///// <returns>Collection of EmploymentFrequency</returns>
        //public async Task<IEnumerable<EmploymentFrequency>> GetEmploymentFrequenciesAsync(bool ignoreCache)
        //{
        //    return await GetGuidValcodeAsync<EmploymentFrequency>("HR", "TIME.FREQUENCIES",
        //        (cl, g) => new EmploymentFrequency(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
        //            ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        //}

        /// <summary>
        /// Get a collection of job change reasons
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of job change reasons</returns>
        public async Task<IEnumerable<JobChangeReason>> GetJobChangeReasonsAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<JobChangeReason>("HR", "POSITION.ENDING.REASONS",
                (e, g) => new JobChangeReason(g, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember),
                bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of rehire types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of rehire types</returns>
        public async Task<IEnumerable<RehireType>> GetRehireTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<RehireType>("HR", "REHIRE.ELIGIBILITY.CODES",
                (e, g) => new RehireType(g, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember,
                    e.ValActionCode3AssocMember), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of HrStatuses
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of HrStatuses</returns>
        public async Task<IEnumerable<HrStatuses>> GetHrStatusesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<HrStatuses>("HR", "HR.STATUSES",
                (cl, g) => new HrStatuses(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember), cl.ValActionCode1AssocMember, cl.ValActionCode3AssocMember), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of earning-types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of earning types</returns>
        public async Task<IEnumerable<EarningType2>> GetEarningTypesAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<Earntype, EarningType2>("AllHREarningTypes", "EARNTYPE",
                (e, g) => new EarningType2(g, e.Recordkey, e.EtpDesc), bypassCache: ignoreCache);
        }

    /// <summary>
    /// Get a collection of employee classifications
    /// </summary>
    /// <param name="ignoreCache">Bypass cache flag</param>
    /// <returns>Collection of employee classifications</returns>
    public async Task<IEnumerable<EmploymentClassification>> GetEmploymentClassificationsAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<EmploymentClassification>("HR", "CLASSIFICATIONS",
                (e, g) => new EmploymentClassification(g, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember,
                    EmploymentClassificationType.Position), bypassCache: ignoreCache);
        }

    /// <summary>
    /// Get all Employment Department objects, built from database data
    /// </summary>
    /// <returns>A list of Employment Department objects</returns>
    public async Task<IEnumerable<EmploymentDepartment>> GetEmploymentDepartmentsAsync(bool ignoreCache = false)
    {
        if (ignoreCache)
        {
            return await BuildAllEmploymentDepartments();
        }
        else
        {
            return await GetOrAddToCacheAsync<IEnumerable<EmploymentDepartment>>("AllEmploymentDepartments", async () => await this.BuildAllEmploymentDepartments(), Level1CacheTimeoutValue);
        }
    }

    private async Task<IEnumerable<EmploymentDepartment>> BuildAllEmploymentDepartments()
    {
        var employmentDepartmentEntities = new List<EmploymentDepartment>();
        var employmentDepartmentIds = await DataReader.SelectAsync("DEPTS", "WITH DEPTS.TYPE NE 'A'");

        var employmentDepartmentRecords = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Depts>(employmentDepartmentIds);

        foreach (var employmentDepartmentRecord in employmentDepartmentRecords)
        {
                try
                {
                    var empDeptGuidInfo = await GetGuidFromRecordInfoAsync("DEPTS", employmentDepartmentRecord.Recordkey, "DEPT.INTG.KEY.IDX", employmentDepartmentRecord.DeptIntgKeyIdx);

                    employmentDepartmentEntities.Add(new EmploymentDepartment(empDeptGuidInfo, employmentDepartmentRecord.Recordkey, !string.IsNullOrEmpty(employmentDepartmentRecord.DeptsDesc) ? employmentDepartmentRecord.DeptsDesc : employmentDepartmentRecord.Recordkey));
                }
                catch (Exception e)
                {
                    throw new KeyNotFoundException("No GUID found for entity DEPTS and ID " + employmentDepartmentRecord.Recordkey);

                }


            }


        return employmentDepartmentEntities;
    }

        /// <summary>
        /// Get a collection of EmploymentPerformanceReviewRating
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of EmploymentPerformanceReviewRating</returns>
        public async Task<IEnumerable<EmploymentPerformanceReviewRating>> GetEmploymentPerformanceReviewRatingsAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<EmploymentPerformanceReviewRating>("HR", "PERFORMANCE.EVAL.RATINGS",
                (cl, g) => new EmploymentPerformanceReviewRating(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of EmploymentPerformanceReviewTypes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of EmploymentPerformanceReviewTypes</returns>
        public async Task<IEnumerable<EmploymentPerformanceReviewType>> GetEmploymentPerformanceReviewTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<EmploymentPerformanceReviewType>("HR", "EVALUATION.CYCLES",
                (cl, g) => new EmploymentPerformanceReviewType(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)) { Frequency = cl.ValActionCode1AssocMember }, bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of employment proficiencies
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of instructional methods</returns>
        public async Task<IEnumerable<EmploymentProficiency>> GetEmploymentProficienciesAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<Jobskills, EmploymentProficiency>("AllEmploymentProficiencies", "JOBSKILLS",
                (ep, g) => new EmploymentProficiency(g, ep.Recordkey, ep.JskDesc) { Certification = ep.JskLicenseCert, Comment = ep.JskComment, Authority = ep.JskAuthority }, bypassCache: ignoreCache);
        }

        /// <summary>
        /// Returns employment status ending reasons.
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<EmploymentStatusEndingReason>> GetEmploymentStatusEndingReasonsAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<EmploymentStatusEndingReason>("HR", "STATUS.ENDING.REASONS",
               (e, g) => new EmploymentStatusEndingReason(g, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember),
               bypassCache: ignoreCache);
        }

        ///// <summary>
        ///// Get a collection of institution job supervisors
        ///// </summary>
        ///// <param name="ignoreCache">Bypass cache flag</param>
        ///// <returns>Collection of institution job supervisors</returns>
        //public async Task<IEnumerable<InstitutionJobSupervisor>> GetInstitutionJobSupervisorsAsync(bool ignoreCache)
        //{
        //    var coreDefaultData = GetDefaults();

        //    return await GetGuidCodeItemAsync<Perpos, InstitutionJobSupervisor>("AllInstitutionJobSupervisors", "PERPOS",
        //        (ijs, g) => new InstitutionJobSupervisor(g, ijs.Recordkey, ijs.RecordModelName, ijs.PerposHrpId, ijs.PerposPositionId) { SupervisorId = ijs.PerposSupervisorHrpId, AlternateSupervisorId = ijs.PerposAltSupervisorId, Employer = coreDefaultData.DefaultHostCorpId }, bypassCache: ignoreCache);
        //}

        /// <summary>
        /// Get a collection of LeaveType
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of LeaveType</returns>
        public async Task<IEnumerable<LeaveType>> GetLeaveTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<LeaveType>("HR", "LEAVE.TYPES",
                (cl, g) => new LeaveType(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)) { TimeType = TranslateTimeType(cl.ValActionCode1AssocMember) }, bypassCache: ignoreCache);
        }

        private LeaveTypeCategory TranslateTimeType(string valActionCode1)
        {
            switch (valActionCode1)
            {
                case "1":
                    return LeaveTypeCategory.Vacation;
                case "2":
                    return LeaveTypeCategory.Sick;
                case "3":
                    return LeaveTypeCategory.Compensatory;
                default:
                    return LeaveTypeCategory.None;
            }
        }

        /// <summary>
        /// Get a collection of Payclass
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of Payclass</returns>
        public async Task<IEnumerable<PayClass>> GetPayClassesAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<Payclass, PayClass>("AllPayClasses", "PAYCLASS",
                (pc, g) => new PayClass(g, pc.Recordkey, pc.PclsDesc)
                {
                    PaysPerYear = pc.PclsCyclesPerYear,
                    PayCycle = pc.PclsPaycycle,
                    PayFrequency = pc.PclsCycleFreq,
                    CycleHoursPerPeriodHours = (decimal)pc.PclsCycleWorkTimeAmt,
                    CycleHoursPerPeriodPeriod = pc.PclsCycleWorkTimeUnits,
                    YearHoursPerPeriodHours = (decimal)pc.PclsYearWorkTimeAmt,
                    YearHoursPerPeriodPeriod = pc.PclsYearWorkTimeUnits,
                    Status = pc.PclsActiveFlag,
                    CompensationType = pc.PclsHrlyOrSlry
                },
                    bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of pay cycles
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of pay cycles</returns>
        public async Task<IEnumerable<PayCycle2>> GetPayCyclesAsync(bool ignoreCache = false)
        {
            if (ignoreCache)
            {
                var payCycles = await BuildAllPayCycles();
                return await AddOrUpdateCacheAsync<IEnumerable<PayCycle2>>("AllPayCycles2", payCycles);

            }
            else
            {
                return await GetOrAddToCacheAsync<IEnumerable<PayCycle2>>("AllPayCycles2", async () => await this.BuildAllPayCycles(), Level1CacheTimeoutValue);
            }
        }

        /// <summary>
        ///  Build PayCycle2 domain entity collection.
        ///  Contains guid lookups to ensure the associated LDM.GUID does not contain a secondary key and/or index
        /// </summary>
        /// <returns>Collection of PayCycle2 domain entities</returns>
        private async Task<IEnumerable<PayCycle2>> BuildAllPayCycles()
        {
            var payCycleEntities = new List<PayCycle2>();
            // exclude any LDM.GUID entries that contain a secondary key and/or index
            string criteria = "WITH LDM.GUID.ENTITY EQ 'PAYCYCLE' AND LDM.GUID.SECONDARY.KEY EQ '' ";

            //retrive string array of applicable guids
            var ldmGuidPayCycle = await DataReader.SelectAsync("LDM.GUID", criteria);
            if ((ldmGuidPayCycle != null) && (ldmGuidPayCycle.Any()))
            {
                // using the string array of applicable guids, convert this into a guidLookup
                var guidLookUp = ldmGuidPayCycle.Select(guid => new GuidLookup(guid)).ToArray();
                if ((guidLookUp == null) || (!guidLookUp.Any()))
                {
                    return payCycleEntities;
                }
                // BulkReadRecord to get a collection of Paycycle data contracts. The guid returned
                // in the data contract may not be correct, therefore additional processing is required.
                var payCycleRecords = await DataReader.BulkReadRecordAsync<Paycycle>("PAYCYCLE", guidLookUp);

                // Perform a guid lookup to get a collection of guids and ids
                var payCycleDictionary = await DataReader.SelectAsync(guidLookUp);

                if ((payCycleDictionary == null) || (!payCycleDictionary.Any()))
                {
                    return payCycleEntities;
                }

                foreach (var payCycleRecord in payCycleRecords)
                {
                    // using the id, retrieve the correct guid from the collection of guids and ids
                    var ldmGuid = payCycleDictionary.FirstOrDefault(id => id.Value != null && payCycleRecord.Recordkey.Equals(id.Value.PrimaryKey, StringComparison.OrdinalIgnoreCase));
                    if (!ldmGuid.Equals(new KeyValuePair<string, GuidLookupResult>()))
                    {
                        payCycleEntities.Add(new PayCycle2(ldmGuid.Key, payCycleRecord.Recordkey, payCycleRecord.PcyDesc) {});
                    }
                }
            }
            return payCycleEntities;
        }


        /// <summary>
        /// Returns payroll deduction arrangement change reasons.
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<PayrollDeductionArrangementChangeReason>> GetPayrollDeductionArrangementChangeReasonsAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<PayrollDeductionArrangementChangeReason>("HR", "BENDED.CHANGE.REASONS",
               (e, g) => new PayrollDeductionArrangementChangeReason(g, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember),
               bypassCache: ignoreCache);
        }

        /// <summary>
        /// Returns HR Person statuses used in PERSTAT.STATUS.
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        //public async Task<IEnumerable<PersonStatuses>> GetPersonStatusesAsync(bool ignoreCache)
        //{
        //    return await GetGuidValcodeAsync<PersonStatuses>("HR", "HR.STATUSES",
        //       (e, g) => new PersonStatuses(g, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember, e.ValActionCode3AssocMember),
        //       bypassCache: ignoreCache);
        //}

        /// <summary>
        /// Get a collection of TenureTypes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of TenureTypes</returns>
        public async Task<IEnumerable<TenureTypes>> GetTenureTypesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<TenureTypes>("HR", "TENURE.TYPES",
                (cl, g) => new TenureTypes(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get the GUID for an Entity
        /// </summary>
        /// <param name="key"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<string> GetGuidFromID(string key, string entity)
        {
            try
            {
                return await GetGuidFromRecordInfoAsync(entity, key);
            }
            catch (RepositoryException REX)
            {
                REX.AddError(new RepositoryError(entity + ".guid.NotFound", "GUID not found for " + entity + "id " + key));
                throw REX;
            }

        }

        /// <summary>
        /// Get the Defaults from CORE to compare default institution Id
        /// </summary>
        /// <returns>Core Defaults</returns>
        private Base.DataContracts.Defaults GetDefaults()
        {
            return GetOrAddToCache<Data.Base.DataContracts.Defaults>("CoreDefaults",
                () =>
                {
                    var coreDefaults = DataReader.ReadRecord<Data.Base.DataContracts.Defaults>("CORE.PARMS", "DEFAULTS");
                    if (coreDefaults == null)
                    {
                        logger.Info("Unable to access DEFAULTS from CORE.PARMS table.");
                        coreDefaults = new Defaults();
                    }
                    return coreDefaults;
                }, Level1CacheTimeoutValue);
        }

        /// <summary>
        /// Get the configuration from HRWEB.DEFAULTS for the pay statement
        /// </summary>
        /// <returns>cached PayStatementConfiguration object</returns>
        public async Task<PayStatementConfiguration> GetPayStatementConfigurationAsync()
        {
            return await GetOrAddToCacheAsync("PayStatementConfiguration", async () => await BuildPayStatementConfigurationAsync(), Level1CacheTimeoutValue);
        }

        /// <summary>
        /// Build the configuration from HRWEB.DEFAULTS for the pay statement
        /// </summary>
        /// <returns>PayStatementConfiguration object</returns>
        private async Task<PayStatementConfiguration> BuildPayStatementConfigurationAsync()
        {
            var hrWebDefaults = await DataReader.ReadRecordAsync<DataContracts.HrwebDefaults>("HR.PARMS", "HRWEB.DEFAULTS");
            var payMaster = await DataReader.ReadRecordAsync<Paymstr>("ACCOUNT.PARAMETERS", "PAYROLL.MASTER");
            var configuration = new PayStatementConfiguration();
            if (hrWebDefaults != null)
            {
                int offsetDaysCount;
                if (int.TryParse(hrWebDefaults.HrwebPayAdviceCheckDate, out offsetDaysCount))
                {
                    configuration.OffsetDaysCount = offsetDaysCount;
                }

                int previousYearsCount;
                if (int.TryParse(hrWebDefaults.HrwebPayAdvicePriorYears, out previousYearsCount))
                {
                    configuration.PreviousYearsCount = previousYearsCount;
                }
                SSNDisplay ssnDisplay;
                if(tryParseSSNDisplay(hrWebDefaults.HrwebPayAdviceDisplaySsn, out ssnDisplay))
                {
                    configuration.SocialSecurityNumberDisplay = ssnDisplay;
                }

                configuration.DisplayWithholdingStatusFlag = string.IsNullOrEmpty(hrWebDefaults.HrwebPayAdviceFilingStat) ||
                    hrWebDefaults.HrwebPayAdviceFilingStat.Equals("Y", StringComparison.InvariantCultureIgnoreCase);
            }
            else
            {
                logger.Info("Null HrwebDefaults record returned from database");
            }

            if (payMaster != null)
            {
                configuration.DisplayZeroAmountBenefitDeductions = !string.IsNullOrEmpty(payMaster.PmZeroBendedOnStub) && payMaster.PmZeroBendedOnStub.Equals("Y", StringComparison.InvariantCultureIgnoreCase);

                //add the address lines
                if (!string.IsNullOrWhiteSpace(payMaster.PmInstitutionName))
                {
                    configuration.InstitutionName = payMaster.PmInstitutionName;
                }
                if (payMaster.PmInstitutionAddress != null)
                {
                    foreach (var addressLine in payMaster.PmInstitutionAddress.Where(a => !string.IsNullOrWhiteSpace(a)))
                    {
                        configuration.InstitutionMailingLabel.Add(new PayStatementAddress(addressLine));
                    }
                }
                if (!string.IsNullOrWhiteSpace(payMaster.PmInstitutionCity) && 
                    !string.IsNullOrWhiteSpace(payMaster.PmInstitutionState) && 
                    !string.IsNullOrWhiteSpace(payMaster.PmInstitutionZipcode))
                {
                    var csz = string.Format("{0}, {1} {2}", payMaster.PmInstitutionCity, payMaster.PmInstitutionState, payMaster.PmInstitutionZipcode);
                    configuration.InstitutionMailingLabel.Add(new PayStatementAddress(csz));
                }
                    
            }
            else
            {
                logger.Info("Null PAYROLL.MASTER record returned from database");
            }

            return configuration;
        }

        private bool tryParseSSNDisplay(string code, out SSNDisplay ssnDisplay)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                ssnDisplay = SSNDisplay.LastFour;
                return false;
            }
            switch(code.ToUpperInvariant())
            {
                case "S": ssnDisplay = SSNDisplay.Full; return true;
                case "L": ssnDisplay = SSNDisplay.LastFour; return true;
                case "N": ssnDisplay = SSNDisplay.Hidden; return true;
                default: ssnDisplay  = SSNDisplay.LastFour; return false;
            }
        }

        /// <summary>
        /// Get all EarningsDifferentials
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<EarningsDifferential>> GetEarningsDifferentialsAsync()
        {
            return await GetOrAddToCacheAsync("EarningsDifferentials", async () =>
            {
                var earningsDifferentials = new List<EarningsDifferential>();

                var records = await DataReader.BulkReadRecordAsync<Earndiff>("");
                if (records == null || !records.Any())
                {
                    logger.Info("No Earnings Differential records were found");
                    return earningsDifferentials;
                }

                foreach (var earnDiffRecord in records)
                {
                    try
                    {
                        earningsDifferentials.Add(new EarningsDifferential(earnDiffRecord.Recordkey, earnDiffRecord.EdfDesc));
                    }
                    catch (Exception e)
                    {
                        LogDataError("EARNDIFF", earnDiffRecord.Recordkey, earnDiffRecord, e);
                    }
                }

                return earningsDifferentials;
            }, Level1CacheTimeoutValue);
        }

        /// <summary>
        /// Gets a list of tax code items
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<TaxCode>> GetTaxCodesAsync()
        {
            return await GetOrAddToCacheAsync<IEnumerable<TaxCode>>("TaxCodes",
                async () => {
                    var codes = await DataReader.BulkReadRecordAsync<Taxcodes>("");
                    var filingStatuses = await GetTaxCodeFilingStatuses();

                    var taxCodes = new List<TaxCode>();
                    foreach (var code in codes)
                    {
                        try
                        {
                            taxCodes.Add(new TaxCode(code.Recordkey, code.TaxDesc, ConvertInternalCode(code.TaxTypeCode))
                            {
                                FilingStatus = filingStatuses.FirstOrDefault(fs => fs.Code == code.TaxFilingStatus)
                            });
                        }
                        catch (Exception e)
                        {
                            LogDataError("Taxcodes", code.Recordkey, code, e);
                        }
                    }
                    return taxCodes;
                }
            );
        }

        /// <summary>
        /// Get a list of Filing Statuses that are assigned to Tax Codes
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<TaxCodeFilingStatus>> GetTaxCodeFilingStatuses()
        {
            return await GetValcodeAsync("HR", "TAXCODE.FILING.STATUSES",
                (vals) => new TaxCodeFilingStatus(vals.ValInternalCodeAssocMember, vals.ValExternalRepresentationAssocMember),
                Level1CacheTimeoutValue);
        }

        private TaxCodeType ConvertInternalCode(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return TaxCodeType.FicaWithholding;
            }

            switch(code.ToUpperInvariant())
            {
                case "FICA":
                    return TaxCodeType.FicaWithholding;
                case "FWHT":
                    return TaxCodeType.FederalWithholding;
                case "EIC":
                    return TaxCodeType.EarnedIncomeCredit;
                case "STATE":
                    return TaxCodeType.StateWithholding;
                case "FUTA":
                    return TaxCodeType.FederalUnemploymentTax;
                case "INSURANCE":
                    return TaxCodeType.UnemployementAndInsurance;
                case "CITY":
                    return TaxCodeType.CityWithholding;
                case "COUNTY":
                    return TaxCodeType.CountyWithholding;
                case "SDIST":
                    return TaxCodeType.SchoolDistrictWithholding;
                case "LOCAL":
                    return TaxCodeType.LocalWithholding;
                case "CPP":
                    return TaxCodeType.CanadianPensionPlan;
                case "UI":
                    return TaxCodeType.CanadianUnemploymentInsurance;
                case "CINC":
                    return TaxCodeType.CanadianFederalIncomeTax;
                case "PROV":
                    return TaxCodeType.CanadianProvincialTax;
                case "WKCOMP":
                    return TaxCodeType.WorkmansCompensation;
                default:
                    return TaxCodeType.FicaWithholding;
            }
        }

        public async Task<IEnumerable<EmploymentFrequency>> GetEmploymentFrequenciesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<EmploymentFrequency>("HR", "TIME.FREQUENCIES",
               (cl, g) => new EmploymentFrequency(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                   ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember), cl.ValActionCode1AssocMember), bypassCache: ignoreCache);
        }



        /// <summary>
        /// Get a collection of Assignment Contract Types
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of instructional methods</returns>
        public async Task<IEnumerable<Domain.HumanResources.Entities.AsgmtContractTypes>> GetAssignmentContractTypesAsync(bool ignoreCache)
        {
            return await GetCodeItemAsync<DataContracts.AsgmtContractTypes, Domain.HumanResources.Entities.AsgmtContractTypes>("AllAssignmentContractTypes", "ASGMT.CONTRACT.TYPES",
                (i) => new Domain.HumanResources.Entities.AsgmtContractTypes(i.Recordkey, i.ActypDesc), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get all the EarningsTypeGroups
        /// </summary>
        /// <returns>Dictionary of EarningsTypeGroups where the key is the earningsTypeGroupId</returns>
        public async Task<IDictionary<string, EarningsTypeGroup>> GetEarningsTypesGroupsAsync()
        {
            return await GetOrAddToCacheAsync("AllEarningsTypeGroups", async () =>
            {
                var earningsTypeGroups = new Dictionary<string, EarningsTypeGroup>();
                var records = await DataReader.BulkReadRecordAsync<DataContracts.EarntypeGroupings>("");
                if (records != null && records.Any())
                {
                    foreach (var record in records)
                    {
                        try
                        {
                            var isEnabledForTimeManagemnt = record.EtpgUseInSelfService != null && 
                                record.EtpgUseInSelfService.Equals("Y", StringComparison.CurrentCultureIgnoreCase);

                            var group = new EarningsTypeGroup(record.Recordkey, record.EtpgDesc, isEnabledForTimeManagemnt)
                            {
                                HolidayCalendarId = record.EtpgHolidayCalendar
                            };
                            for (var i = 0; i < record.EtpgEarntype.Count; i++)
                            {
                                try
                                {
                                    var entry = new EarningsTypeGroupItem(record.EtpgEarntype[i], record.EtpgEarntypeDesc[i], record.Recordkey);
                                    group.TryAdd(entry);
                                }
                                catch (Exception e)
                                {
                                    LogDataError("EARNTYPE.GROUPINGS", record.Recordkey, record, e, "Unable to add EarningsTypeGroupItem earnTypeId: " + record.EtpgEarntype[i]);
                                }
                            }

                            if (!earningsTypeGroups.ContainsKey(group.EarningsTypeGroupId))
                            {
                                earningsTypeGroups.Add(group.EarningsTypeGroupId, group);
                            }
                        }
                        catch (Exception e)
                        {
                            LogDataError("EARNTYPE.GROUPINGS", record.Recordkey, record, e, "Something is wrong with this record: " + record.Recordkey);
                        }
                    }
                }

                return earningsTypeGroups;                
            });
        }

        //public string ConvertEmploymentFrequenciesType(string type)
        //{
        //    switch (type)
        //    {
        //        case "365":
        //            return "daily";
        //        case "52":
        //            return "weekly";
        //        case "24":
        //            return "semimonthly";
        //        case "26":
        //            return "biweekly";
        //        case "12":
        //            return "monthly";
        //        case "4":
        //            return "quarterly";
        //        case "1":
        //            return "annually";
        //    }

        //    return null;
        //}
    }
}
