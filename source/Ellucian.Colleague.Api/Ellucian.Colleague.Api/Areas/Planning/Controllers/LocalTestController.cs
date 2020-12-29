// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Areas.Planning.Models.Tests;
using Ellucian.Colleague.Configuration;
using Ellucian.Colleague.Coordination.Planning.Services;
using Ellucian.Colleague.Data.Base;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Planning.Repositories;
using Ellucian.Colleague.Data.Student;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Dmi.Client;
using Ellucian.Dmi.Client.DMIF;
using Ellucian.Logging;
using Ellucian.Web.Adapters;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Mvc.Filter;
using Ellucian.Web.Security;
using Microsoft.IdentityModel.Claims;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Ellucian.Dmi.Client.DMIF;

namespace Ellucian.Colleague.Api.Areas.Planning.Controllers
{
    /// <summary>
    /// Test utilities controller for planning module.
    /// </summary>
    [LocalRequest]
    public class LocalTestController : Controller
    {
        private static string TxFactoryKey = "TxFactory";
        private StringLogger logger = new StringLogger();
        private ApiSettings apiSettingsMock;
        private ColleagueSettings colleagueSettingsMock;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <returns></returns>
        [ActionName("ScanRules")]
        public async Task<ActionResult> ScanRulesAsync()
        {
            Setup();
            var results = await QueryRulesAsync(GetTxFactory());
            results.Log = logger.ToString();
            return View(results);
        }

        private void Setup()
        {
            logger.Clear();
            var cookie = LocalUserUtilities.GetCookie(Request);
            var cookieValue = cookie == null ? null : cookie.Value;
            if (string.IsNullOrEmpty(cookieValue))
            {
                throw new Exception("Log in first");
            }
            var baseUrl = cookieValue.Split('*')[0];
            var token = cookieValue.Split('*')[1];
            var principal = JwtHelper.CreatePrincipal(token);
            var sessionClaim = (principal as IClaimsPrincipal).Identities.First().Claims.FirstOrDefault(c => c.ClaimType == "sid");
            var personId = (principal as IClaimsPrincipal).Identities.First().Claims.FirstOrDefault(c => c.ClaimType == "pid").Value;
            var securityToken = sessionClaim.Value.Split('*')[0];
            var controlId = sessionClaim.Value.Split('*')[1];
            var session = new StandardDmiSession() { SecurityToken = securityToken, SenderControlId = controlId };
            var settings = DependencyResolver.Current.GetService<DmiSettings>();
            var txFactory = new TestTransactionFactory(session, logger, settings);
            HttpContext.Items[TxFactoryKey] = txFactory;
            HttpContext.Items[TestController.PersonIdKey] = personId;
        }

        private IColleagueTransactionFactory GetTxFactory()
        {
            return HttpContext.Items[TxFactoryKey] as IColleagueTransactionFactory;
        }

        private string GetPersonId()
        {
            return HttpContext.Items[TestController.PersonIdKey] as string;
        }

        /// <summary>
        /// Gets the evaluation test page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Evaluation()
        {
            return View();
        }

        /// <summary>
        /// Submits an evaluation request.
        /// </summary>
        /// <param name="testEvaluation"><see cref="TestEvaluation"/> model</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Evaluation(TestEvaluation testEvaluation)
        {
            TempData["testEval"] = testEvaluation;
            return RedirectToAction("EvaluationResult");
        }

        /// <summary>
        /// Gets the evaluation result page.
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> EvaluationResult()
        {
            Setup();
            var txFactory = GetTxFactory();
            var cacheProvider = new MemoryCacheProvider();
            var ruleAdapterRegistry = new RuleAdapterRegistry();
            apiSettingsMock = new ApiSettings("null");
            colleagueSettingsMock = new ColleagueSettings();
            ruleAdapterRegistry.Register<Course>("COURSES", new CourseRuleAdapter());
            ruleAdapterRegistry.Register<AcademicCredit>("STUDENT.ACAD.CRED", new AcademicCreditRuleAdapter());
            var ruleRepo = new RuleRepository(cacheProvider, txFactory, logger, ruleAdapterRegistry, new RuleConfiguration());
            var gradeRepo = new GradeRepository(cacheProvider, txFactory, logger);
            var requirementRepo = new RequirementRepository(cacheProvider, txFactory, logger, gradeRepo, ruleRepo);
            var programRequirementsRepo = new ProgramRequirementsRepository(cacheProvider, txFactory, logger, requirementRepo, gradeRepo, ruleRepo);
            var studentRepo = new StudentRepository(cacheProvider, txFactory, logger, apiSettingsMock);
            var studentProgramRepo = new StudentProgramRepository(cacheProvider, txFactory, logger, apiSettingsMock);
            var courseRepo = new CourseRepository(cacheProvider, txFactory, logger, apiSettingsMock);
            var termRepo = new TermRepository(cacheProvider, txFactory, logger);
            var academicCreditRepo = new AcademicCreditRepository(cacheProvider, txFactory, logger, courseRepo, gradeRepo, termRepo, apiSettingsMock);
            var degreePlanRepo = new DegreePlanRepository(cacheProvider, txFactory, logger, apiSettingsMock);
            var studentDegreePlanRepo = new StudentDegreePlanRepository(cacheProvider, txFactory, logger, apiSettingsMock);
            var programRepo = new ProgramRepository(cacheProvider, txFactory, logger, apiSettingsMock);
            var catalogRepo = new CatalogRepository(cacheProvider, txFactory, logger);
            var planningConfigRepo = new PlanningConfigurationRepository(cacheProvider, txFactory, logger);
            var planningStudentRepo = new PlanningStudentRepository(cacheProvider, txFactory, logger, apiSettingsMock);
            var referenceDataRepo = new ReferenceDataRepository(cacheProvider, txFactory, logger, apiSettingsMock);
            var baseConfigRepo = new ConfigurationRepository(cacheProvider, txFactory, apiSettingsMock, logger, colleagueSettingsMock);
            ICurrentUserFactory currentUserFactory = DependencyResolver.Current.GetService<ICurrentUserFactory>();
            IRoleRepository roleRepository = DependencyResolver.Current.GetService<IRoleRepository>();
            

            //dumper = new Dumper();
            var programEvaluationService = new ProgramEvaluationService(
                new AdapterRegistry(new HashSet<ITypeAdapter>(), logger), studentDegreePlanRepo, programRequirementsRepo, studentRepo, 
                planningStudentRepo, studentProgramRepo, requirementRepo, academicCreditRepo, degreePlanRepo, courseRepo, 
                termRepo, ruleRepo, programRepo, catalogRepo, planningConfigRepo,referenceDataRepo, 
                currentUserFactory, roleRepository, logger, baseConfigRepo);

            var result = new TestEvaluationResult();
            result.PersonId = GetPersonId();
            result.ProgramId = (TempData["testEval"] as TestEvaluation).Program.ToUpper();
            var evaluation = (await programEvaluationService.EvaluateAsync(GetPersonId(), new List<string>() {result.ProgramId.ToUpper()}, null)).First();
            result.Evaluation = evaluation.ToString();
            result.Log = logger.ToString();

            return View(result);
        }

        private async Task<TestScanRules> QueryRulesAsync(IColleagueTransactionFactory txFactory)
        {
            var results = new TestScanRules();
            //results.Name = env;
            var cacheProvider = new MemoryCacheProvider();
            var ruleIds = new List<string>();
            ruleIds.AddRange(txFactory.GetDataReader().Select("ACAD.REQMT.BLOCKS", "ACRB.ACAD.CRED.RULES NE '' SAVING ACRB.ACAD.CRED.RULES"));
            ruleIds.AddRange(txFactory.GetDataReader().Select("ACAD.REQMT.BLOCKS", "ACRB.MAX.COURSES.RULES NE '' SAVING ACRB.MAX.COURSES.RULES"));
            ruleIds.AddRange(txFactory.GetDataReader().Select("ACAD.REQMT.BLOCKS", "ACRB.MAX.CRED.RULES NE '' SAVING ACRB.MAX.CRED.RULES"));
            ruleIds.AddRange(txFactory.GetDataReader().Select("ACAD.PROGRAM.REQMTS", "ACPR.ACAD.CRED.RULES NE '' SAVING ACPR.ACAD.CRED.RULES"));
            ruleIds = ruleIds.Distinct().Where(r => !r.Contains("ý")).ToList();
            var ruleAdapterRegistry = new RuleAdapterRegistry();
            ruleAdapterRegistry.Register<AcademicCredit>("STUDENT.ACAD.CRED", new AcademicCreditRuleAdapter());
            ruleAdapterRegistry.Register<Course>("COURSES", new CourseRuleAdapter());
            var ruleRepository = new RuleRepository(cacheProvider, txFactory, logger, ruleAdapterRegistry, new RuleConfiguration());
            var rules = await ruleRepository.GetManyAsync(ruleIds);
            results.Count = rules.Count();
            foreach (var rule in rules)
            {
                if (rule.GetType() == typeof(Rule<Course>))
                {
                    var crule = (Rule<Course>)rule;
                    if (crule.HasExpression)
                    {
                        results.Supported++;
                    }
                    else
                    {
                        results.NotSupportedNames.Add(crule);
                    }
                }
                else if (rule.GetType() == typeof(Rule<AcademicCredit>))
                {
                    var stcRule = (Rule<AcademicCredit>)rule;
                    if (stcRule.HasExpression)
                    {
                        results.Supported++;
                    }
                    else
                    {
                        results.NotSupportedNames.Add(stcRule);
                    }
                }
            }


            var rawRecords = txFactory.GetDataReader().BulkReadRecord<Rules>("RULES", results.NotSupportedNames.Select(rr => rr.Id).ToArray());
            foreach (var record in rawRecords)
            {
                logger.Info(RuleRepository.Dump(record));
                logger.Info("Not supported because: " + results.NotSupportedNames.First(rr => rr.Id == record.Recordkey).NotSupportedMessage);
            }
            logger.Info(" ");
            return results;
        }

        class TestTransactionFactory : IColleagueTransactionFactory
        {
            private StandardDmiSession session;
            private ILogger logger;
            private DmiSettings settings;

            public TestTransactionFactory(StandardDmiSession session, ILogger logger, DmiSettings settings)
            {
                this.session = session;
                this.logger = logger;
                this.settings = settings;
            }

            public IColleagueDataReader GetColleagueDataReader()
            {
                return new ColleagueDataReader(session, settings);
            }

            public IColleagueDataReader GetDataReader()
            {
                return GetDataReader(false);
            }

            public IColleagueDataReader GetDataReader(bool anonymous)
            {
                if (anonymous)
                {
                    return new AnonymousColleagueDataReader(settings);
                }
                else
                {
                    return new ColleagueDataReader(session, settings);
                }
            }

            public IColleagueTransactionInvoker GetTransactionInvoker()
            {
                return new ColleagueTransactionInvoker(session.SecurityToken, session.SenderControlId, logger, settings);
            }

            public DMIFileTransferClient GetDMIFClient()
            {
                return new DMIFileTransferClient(settings, logger);
            }
        }
    }
}
