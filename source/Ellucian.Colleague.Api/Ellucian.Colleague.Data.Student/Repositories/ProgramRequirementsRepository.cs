// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class ProgramRequirementsRepository : BaseColleagueRepository, IProgramRequirementsRepository
    {

        private IGradeRepository gradeRepo;
        private IRequirementRepository requirementRepo;
        private readonly IRuleRepository ruleRepository;

        public ProgramRequirementsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, IRequirementRepository requirementRepo, IGradeRepository gradeRepo, IRuleRepository ruleRepository)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
            this.gradeRepo = gradeRepo;
            this.requirementRepo = requirementRepo;

            if (ruleRepository == null)
            {
                throw new ArgumentNullException("ruleRepository");
            }
            this.ruleRepository = ruleRepository;
        }

        public async Task<ProgramRequirements> GetAsync(string prog, string cat)
        {
            string filekey = prog + "*" + cat;
            try
            {
                var programRequirementsData = await GetOrAddToCacheAsync<ProgramRequirements>("ProgramRequirements*" + filekey,
                    async () =>
                    {
                        ProgramRequirements pr = await BuildProgramRequirementsAsync(prog, cat);
                        return pr;
                    }
                    );
                return programRequirementsData;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("No requirments were found for the program {0} and catalog {1}", prog, cat));
                //returning null as no requirements found.
                return null;
            }
        }


        private async Task<ProgramRequirements> BuildProgramRequirementsAsync(string prog, string cat)
        {
            var grades = await gradeRepo.GetAsync();
            ProgramRequirements pr = new ProgramRequirements(prog, cat);
            string filekey = prog + "*" + cat;

            //ACAD.PROGRAM.REQMTS
            var acadProgramReqmt = await DataReader.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", filekey);

            if (acadProgramReqmt == null)
            {
                logger.Info("Program '" + prog + "' for catalog '" + cat + "'" + " missing ACAD.PROGRAM.REQMTS record.");
                return null;
            }


            pr.ActivityEligibilityRules = new List<RequirementRule>();
            if (acadProgramReqmt.AcprAcadCredRules != null)
            {
                var ruleIds = acadProgramReqmt.AcprAcadCredRules.Where(r => !string.IsNullOrEmpty(r));
                var rules = await ruleRepository.GetManyAsync(ruleIds);
                foreach (var rule in rules)
                {
                    var requirementRule = RequirementRule.TryCreate(rule);
                    if (requirementRule == null)
                    {
                        // Rule not written against STC or COURSES
                        logger.Info("Rule " + rule.Id + " ignored on program " + pr.ProgramCode);
                    }
                    else
                    {
                        pr.ActivityEligibilityRules.Add(requirementRule);
                    }
                }
            }
            pr.MaximumCredits = acadProgramReqmt.AcprMaxCred;
            if (acadProgramReqmt.AcprMinGrade != null && acadProgramReqmt.AcprMinGrade != "")
            {
                pr.MinGrade = grades.Where(g => g.Id == acadProgramReqmt.AcprMinGrade).First();
            }

            pr.MinimumInstitutionalCredits = acadProgramReqmt.AcprInstitutionCred;
            pr.MinInstGpa = acadProgramReqmt.AcprInstitutionGpa;
            pr.MinimumCredits = acadProgramReqmt.AcprCred;
            pr.MinOverallGpa = acadProgramReqmt.AcprMinGpa;
            pr.CurriculumTrackCode = acadProgramReqmt.AcprCurriculumTrack;

            List<Grade> othergrades = new List<Grade>();
            if (acadProgramReqmt.AcprOtherGrades != null)
            {
                foreach (var grade in acadProgramReqmt.AcprOtherGrades)
                {
                    if (grade != null && grade != "")
                    {
                        othergrades.Add(grades.Where(g => g.Id == grade).FirstOrDefault());
                    }
                }
            }
            pr.AllowedGrades = othergrades;

            // Requirements list
            pr.Requirements = new List<Requirement>();
            var requirements = await requirementRepo.GetAsync(acadProgramReqmt.AcprAcadReqmts, pr);
            foreach (var req in requirements)  // Make a new Get() to pass in the min grade stuff from the PR
            {
                pr.Requirements.Add(req);
            }

            // Set insertion points for additional CCDs, Majors, Minors, and Specializations
            pr.RequirementToPrintCcdsAfter = acadProgramReqmt.AcprAddnlCcdPl;
            pr.RequirementToPrintMajorsAfter = acadProgramReqmt.AcprAddnlMajorPl;
            pr.RequirementToPrintMinorsAfter = acadProgramReqmt.AcprAddnlMinorPl;
            pr.RequirementToPrintSpecializationsAfter = acadProgramReqmt.AcprAddnlSpecializationPl;

            return pr;
        }

        /// <summary>
        /// Wrapper around Async, used by FinancialAid branch for AcademicProgressService
        /// </summary>
        /// <param name="prog"></param>
        /// <param name="cat"></param>
        /// <returns></returns>
        public ProgramRequirements Get(string prog, string cat)
        {
            var x = Task.Run(async () =>
            {
                return await GetAsync(prog, cat);
            }).GetAwaiter().GetResult();
            return x;
        }
    }
}
