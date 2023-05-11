/*Copyright 2015-2020 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Coordinates getting repository data to build ShoppingSheet Dtos
    /// </summary>
    [RegisterType]
    public class ShoppingSheetService : AwardYearCoordinationService, IShoppingSheetService
    {
        private readonly IFinancialAidReferenceDataRepository financialAidReferenceDataRepository;
        private readonly IStudentAwardRepository studentAwardRepository;
        private readonly IStudentBudgetComponentRepository studentBudgetComponentRepository;
        private readonly IFafsaRepository fafsaRepository;
        private readonly IProfileApplicationRepository profileApplicationRepository;
        private readonly IRuleTableRepository ruleTableRepository;
        private readonly IRuleRepository ruleRepository;
        private readonly IConfigurationRepository configurationRepository;

        public ShoppingSheetService(
            IAdapterRegistry adapterRegistry,
            IFinancialAidOfficeRepository financialAidOfficeRepository,
            IStudentAwardYearRepository studentAwardYearRepository,
            IFinancialAidReferenceDataRepository financialAidReferenceDataRepository,
            IStudentAwardRepository studentAwardRepository,
            IStudentBudgetComponentRepository studentBudgetComponentRepository,
            IFafsaRepository fafsaRepository,
            IProfileApplicationRepository profileApplicationRepository,
            IRuleTableRepository ruleTableRepository,
            IRuleRepository ruleRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, financialAidOfficeRepository, studentAwardYearRepository, configurationRepository, currentUserFactory, roleRepository, logger)
        {
            this.financialAidReferenceDataRepository = financialAidReferenceDataRepository;
            this.studentAwardRepository = studentAwardRepository;
            this.studentBudgetComponentRepository = studentBudgetComponentRepository;
            this.fafsaRepository = fafsaRepository;
            this.profileApplicationRepository = profileApplicationRepository;
            this.ruleTableRepository = ruleTableRepository;
            this.ruleRepository = ruleRepository;
            this.configurationRepository = configurationRepository;
        }

        /// <summary>
        /// Get a collection of Student specific Shopping Sheets
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get shopping sheets</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only</param>
        /// <returns>A list of ShoppingSheets</returns>
        public async Task<IEnumerable<Dtos.FinancialAid.ShoppingSheet>> GetShoppingSheetsAsync(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to access shopping sheet resources for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var studentAwardYears = await GetStudentAwardYearEntitiesAsync(studentId, getActiveYearsOnly);
            if (studentAwardYears == null || studentAwardYears.Count() == 0)
            {
                logger.Debug(string.Format("Student {0} has no award years for which to get shopping sheets", studentId));
                return new List<Dtos.FinancialAid.ShoppingSheet>();
            }

            //its ok if budgetComponents and studentBudgetComponents are null
            var budgetComponents = financialAidReferenceDataRepository.BudgetComponents;
            var studentBudgetComponents = await studentBudgetComponentRepository.GetStudentBudgetComponentsAsync(studentId, studentAwardYears);

            //its ok if studentAwards are null
            var studentAwards = await studentAwardRepository.GetAllStudentAwardsAsync(studentId, studentAwardYears, financialAidReferenceDataRepository.Awards, financialAidReferenceDataRepository.AwardStatuses);

            var financialAidApplications = new List<Domain.FinancialAid.Entities.FinancialAidApplication2>();
            var fafsas = await fafsaRepository.GetFafsasAsync(new List<string>() { studentId }, studentAwardYears.Select(y => y.Code));
            if (fafsas != null)
            {
                financialAidApplications.AddRange(fafsas);
            }

            var profileApplications = await profileApplicationRepository.GetProfileApplicationsAsync(studentId, studentAwardYears);
            if (profileApplications != null)
            {
                financialAidApplications.AddRange(profileApplications);
            }

            var shoppingSheetRuleTables = await ruleTableRepository.GetShoppingSheetRuleTablesAsync(studentAwardYears.Select(y => y.Code));
            if (shoppingSheetRuleTables != null)
            {
                var rules = await ruleRepository.GetManyAsync(
                    shoppingSheetRuleTables.SelectMany(ruleTable => ruleTable.RuleIds));

                foreach (var ruleTable in shoppingSheetRuleTables)
                {
                    ruleTable.RuleProcessor = new Func<IEnumerable<RuleRequest<Domain.FinancialAid.Entities.StudentAwardYear>>, Task<IEnumerable<RuleResult>>>(
                        async(ruleRequests) =>
                            (await ruleRepository.ExecuteAsync(ruleRequests)));

                    ruleTable.LinkRuleObjects(rules);
                }
            }


            var shoppingSheets = new List<Domain.FinancialAid.Entities.ShoppingSheet>();
            foreach (var studentAwardYear in studentAwardYears)
            {
                try
                {
                    shoppingSheets.Add(
                        await ShoppingSheetDomainService.BuildShoppingSheetAsync(studentAwardYear, studentAwards, budgetComponents, studentBudgetComponents, financialAidApplications, shoppingSheetRuleTables));
                }
                catch (Exception e)
                {
                    logger.Debug(e, "Unable to create shopping sheet for studentId {0} and awardYear {1}", studentId, studentAwardYear.Code);
                }
            }

            var shoppingSheetDtoAdapter = _adapterRegistry.GetAdapter<Colleague.Domain.FinancialAid.Entities.ShoppingSheet, Colleague.Dtos.FinancialAid.ShoppingSheet>();

            return shoppingSheets.Select(shoppingSheetEnitity =>
                shoppingSheetDtoAdapter.MapToType(shoppingSheetEnitity));
        }

        /// <summary>
        /// Get a collection of Student specific Shopping Sheets
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get shopping sheets</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only</param>
        /// <returns>A list of ShoppingSheets</returns>
        public async Task<IEnumerable<Dtos.FinancialAid.ShoppingSheet2>> GetShoppingSheets2Async(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to access shopping sheet resources for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var studentAwardYears = await GetStudentAwardYearEntitiesAsync(studentId, getActiveYearsOnly);
            if (studentAwardYears == null || studentAwardYears.Count() == 0)
            {
                logger.Debug(string.Format("Student {0} has no award years for which to get shopping sheets", studentId));
                return new List<Dtos.FinancialAid.ShoppingSheet2>();
            }

            //its ok if budgetComponents and studentBudgetComponents are null
            var budgetComponents = financialAidReferenceDataRepository.BudgetComponents;
            var studentBudgetComponents = await studentBudgetComponentRepository.GetStudentBudgetComponentsAsync(studentId, studentAwardYears);

            //its ok if studentAwards are null
            var studentAwards = await studentAwardRepository.GetAllStudentAwardsAsync(studentId, studentAwardYears, financialAidReferenceDataRepository.Awards, financialAidReferenceDataRepository.AwardStatuses);

            var financialAidApplications = new List<Domain.FinancialAid.Entities.FinancialAidApplication2>();
            var fafsas = await fafsaRepository.GetFafsasAsync(new List<string>() { studentId }, studentAwardYears.Select(y => y.Code));
            if (fafsas != null)
            {
                financialAidApplications.AddRange(fafsas);
            }

            var profileApplications = await profileApplicationRepository.GetProfileApplicationsAsync(studentId, studentAwardYears);
            if (profileApplications != null)
            {
                financialAidApplications.AddRange(profileApplications);
            }

            var shoppingSheetRuleTables = await ruleTableRepository.GetShoppingSheetRuleTablesAsync(studentAwardYears.Select(y => y.Code));
            if (shoppingSheetRuleTables != null)
            {
                var rules = await ruleRepository.GetManyAsync(
                    shoppingSheetRuleTables.SelectMany(ruleTable => ruleTable.RuleIds));

                foreach (var ruleTable in shoppingSheetRuleTables)
                {
                    ruleTable.RuleProcessor = new Func<IEnumerable<RuleRequest<Domain.FinancialAid.Entities.StudentAwardYear>>, Task<IEnumerable<RuleResult>>>(
                        async (ruleRequests) =>
                            (await ruleRepository.ExecuteAsync(ruleRequests)));

                    ruleTable.LinkRuleObjects(rules);
                }
            }

            var shoppingSheets = new List<Domain.FinancialAid.Entities.ShoppingSheet2>();
            foreach (var studentAwardYear in studentAwardYears)
            {
                var awardYearString = studentAwardYear.ToString();
                var cfpVersion = await studentAwardRepository.GetCFPVersionAsync(studentId, awardYearString);

                try
                {
                    shoppingSheets.Add(
                        await ShoppingSheetDomainService.BuildShoppingSheet2Async(studentAwardYear, studentAwards, budgetComponents, studentBudgetComponents, financialAidApplications, shoppingSheetRuleTables));
                }
                catch (Exception e)
                {
                    logger.Debug(e, "Unable to create shopping sheet for studentId {0} and awardYear {1}", studentId, studentAwardYear.Code);
                }
            }

            var shoppingSheetDtoAdapter = _adapterRegistry.GetAdapter<Colleague.Domain.FinancialAid.Entities.ShoppingSheet2, Colleague.Dtos.FinancialAid.ShoppingSheet2>();

            return shoppingSheets.Select(shoppingSheetEnitity =>
                shoppingSheetDtoAdapter.MapToType(shoppingSheetEnitity));
        }

        /// <summary>
        /// Get a collection of Student specific Shopping Sheets
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get shopping sheets</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only</param>
        /// <returns>A list of ShoppingSheets</returns>
        public async Task<IEnumerable<Dtos.FinancialAid.ShoppingSheet3>> GetShoppingSheets3Async(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to access shopping sheet resources for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var studentAwardYears = await GetStudentAwardYearEntitiesAsync(studentId, getActiveYearsOnly);
            if (studentAwardYears == null || studentAwardYears.Count() == 0)
            {
                logger.Debug(string.Format("Student {0} has no award years for which to get shopping sheets", studentId));
                return new List<Dtos.FinancialAid.ShoppingSheet3>();
            }

            //its ok if budgetComponents and studentBudgetComponents are null
            var budgetComponents = financialAidReferenceDataRepository.BudgetComponents;
            var studentBudgetComponents = new List <StudentBudgetComponent>();
            try
            {
                studentBudgetComponents = (List<StudentBudgetComponent>)await studentBudgetComponentRepository.GetStudentBudgetComponentsAsync(studentId, studentAwardYears);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            //its ok if studentAwards are null
            var studentAwards = await studentAwardRepository.GetAllStudentAwardsAsync(studentId, studentAwardYears, financialAidReferenceDataRepository.Awards, financialAidReferenceDataRepository.AwardStatuses);

            var financialAidApplications = new List<Domain.FinancialAid.Entities.FinancialAidApplication2>();
            var fafsas = await fafsaRepository.GetFafsasAsync(new List<string>() { studentId }, studentAwardYears.Select(y => y.Code));
            if (fafsas != null)
            {
                financialAidApplications.AddRange(fafsas);
            }

            var profileEfc = await fafsaRepository.GetEfcAsync(studentId, studentAwardYears);

            var fafsaEfc = await fafsaRepository.GetFafsaEfcAsync(studentId, studentAwardYears);

            var profileApplications = await profileApplicationRepository.GetProfileApplicationsAsync(studentId, studentAwardYears);
            if (profileApplications != null)
            {
                financialAidApplications.AddRange(profileApplications);
            }

            var shoppingSheetRuleTables = await ruleTableRepository.GetShoppingSheetRuleTablesAsync(studentAwardYears.Select(y => y.Code));
            if (shoppingSheetRuleTables != null)
            {
                var rules = await ruleRepository.GetManyAsync(
                    shoppingSheetRuleTables.SelectMany(ruleTable => ruleTable.RuleIds));

                foreach (var ruleTable in shoppingSheetRuleTables)
                {
                    if (string.IsNullOrEmpty(ruleTable.RtSubrName))
                    {
                        ruleTable.RuleProcessor = new Func<IEnumerable<RuleRequest<Domain.FinancialAid.Entities.StudentAwardYear>>, Task<IEnumerable<RuleResult>>>(
                            async (ruleRequests) =>
                                (await ruleRepository.ExecuteAsync(ruleRequests)));

                        ruleTable.LinkRuleObjects(rules);
                    } else if (!string.IsNullOrEmpty(ruleTable.RtSubrName))
                    {
                        logger.Info(string.Format("Custom verbiage subroutine found for rule table {0}", ruleTable.Code));
                        try
                        {
                            await ruleTableRepository.GetCustomVerbiageAsync(ruleTable, studentId);
                        }
                        catch(ColleagueSessionExpiredException) 
                        { 
                            throw; 
                        }
                    }
                }
            }

            var shoppingSheets = new List<Domain.FinancialAid.Entities.ShoppingSheet3>();
            foreach (var studentAwardYear in studentAwardYears)
            {
                var awardYearString = studentAwardYear.ToString();
                var cfpVersion = await studentAwardRepository.GetCFPVersionAsync(studentId, awardYearString);

                try
                {
                    if (Convert.ToInt32(awardYearString) <= 2021)
                    {
                        shoppingSheets.Add(
                            await ShoppingSheetDomainService.BuildShoppingSheet3Async(studentAwardYear, studentAwards, budgetComponents, studentBudgetComponents, financialAidApplications, shoppingSheetRuleTables, cfpVersion, profileEfc, fafsaEfc));
                    }
                    else
                    {
                        //If the year is 2022 and the flag on CFPP3 is set to use VETS information, attempt to retrieve any VETS amounts for this student/year to pass into the shopping sheet creation
                        var vetBenVar = studentAwardYear.CurrentConfiguration.ShoppingSheetConfiguration.UseVetsData;
                        int? vetBenAmt = null;
                        if (!string.IsNullOrEmpty(vetBenVar) && vetBenVar.ToUpper() == "Y")
                        {
                            vetBenAmt = studentAwardRepository.GetVetBenAmount(studentId, awardYearString, cfpVersion);
                        }
                        shoppingSheets.Add(
                            await ShoppingSheetDomainService.BuildShoppingSheet4Async(studentAwardYear, studentAwards, budgetComponents, studentBudgetComponents, financialAidApplications, shoppingSheetRuleTables, cfpVersion, profileEfc, fafsaEfc, vetBenAmt));
                    }
                }
                catch (Exception e)
                {
                    logger.Debug(e, "Unable to create shopping sheet for studentId {0} and awardYear {1}", studentId, studentAwardYear.Code);
                }
            }

            var shoppingSheetDtoAdapter = _adapterRegistry.GetAdapter<Colleague.Domain.FinancialAid.Entities.ShoppingSheet3, Colleague.Dtos.FinancialAid.ShoppingSheet3>();

            return shoppingSheets.Select(shoppingSheetEnitity =>
                shoppingSheetDtoAdapter.MapToType(shoppingSheetEnitity));
        }
    }
}
