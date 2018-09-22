// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
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
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class RequirementRepository : BaseColleagueRepository, IRequirementRepository
    {
        private const string _IgnoreCode = "I";
        private const string _DisplayCode = "D";
        private const string _SemiApplyCode = "S";
        private const string _ApplyCode = "A";
        private readonly IGradeRepository gradeRepository;
        private readonly IRuleRepository ruleRepository;

        public RequirementRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, IGradeRepository gradeRepository, IRuleRepository ruleRepository)
            : base(cacheProvider, transactionFactory, logger)
        {
            // setting to level 1 timeout for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
            this.gradeRepository = gradeRepository;

            if (ruleRepository == null)
            {
                throw new ArgumentNullException("ruleRepository");
            }
            this.ruleRepository = ruleRepository;
        }


        public async Task<Requirement> GetAsync(string requirementCode)
        {
            if (string.IsNullOrEmpty(requirementCode))
            {
                throw new ArgumentNullException("Requirement ID must be specified");
            }

            try
            {
                IEnumerable<string> codelist = new List<string>() { requirementCode };
                return (await GetAsync(codelist)).First();
            }
            catch (Exception ex)
            {
                if (logger.IsErrorEnabled)
                {
                    // Log the exception
                    var errorMessage = "Requirement record not found for code " + requirementCode;
                    logger.Error(ex.ToString());
                    logger.Error(errorMessage);
                }
                return null;
            }

        }

        /// <summary>
        /// Returns requirements associated with the given academic program requirement object
        /// </summary>
        /// <param name="requirementCodes">List of Ids of the requirements in the given program</param>
        /// <param name="programRequirements">ProgramRequirements object to which the returned programs are to be linked</param>
        /// <returns>A ProgramRequirements object with links to requirements</returns>
        public async Task<IEnumerable<Requirement>> GetAsync(IEnumerable<string> requirementCodes, ProgramRequirements programRequirements)
        {
            IEnumerable<Requirement> requirements = await GetAsync(requirementCodes);
            foreach (var req in requirements)
            {
                req.ProgramRequirements = programRequirements;

                // If MinGrade not provided on requirement, inherit MinGrade and AllowedGrades from program requirements
                if (req.MinGrade == null && programRequirements != null)
                {
                    req.MinGrade = programRequirements.MinGrade;
                    req.AllowedGrades = programRequirements.AllowedGrades.ToList();
                    // If min/allowed grades inherited from program requirements, cascade down through subrequirements and groups
                    CascadeInheritedGrades(req);
                }
            }
            return requirements;
        }


        /// <summary>
        /// Get the requirements with the given Ids
        /// </summary>
        /// <param name="requirementCodes">List of Ids of Requirements to retrieve</param>
        /// <returns>List of Requirement objects</returns>
        public async Task<IEnumerable<Requirement>> GetAsync(IEnumerable<string> requirementCodes)
        {
            // Make sure we have a clean list of requirement Ids.
            if (requirementCodes == null || requirementCodes.Count() <= 0)
            {
                return new List<Requirement>();
            }
            requirementCodes = requirementCodes.Where(r => !string.IsNullOrEmpty(r)).Distinct();
            ISet<Requirement> cached = new HashSet<Requirement>();
            foreach (var requirementCode in requirementCodes)
            {
                var cachedReq = _cacheProvider.Get(BuildFullCacheKey(requirementCode));
                if (cachedReq != null)
                {
                    cached.Add((Requirement)cachedReq);
                }
                else
                {
                    break;
                }
            }
            if (cached.Count == requirementCodes.Count())
            {
                return cached;
            }

            // Create a hash set of requirements and succeeding blocks
            // This is the collection that will be returned.
            ISet<Requirement> reqs = new HashSet<Requirement>();

            if (requirementCodes != null && requirementCodes.Count() > 0)
            {
                var grades = await gradeRepository.GetAsync();

                //retrieve parameter that define whether "my progress" display text should show similar to html version  of EVAL or 
                //show the way as it is entered
                bool printTextAsEntered =await RetrievePrintTextParamAsync();

                // Requirements moved to a separate repository because of multiple entry points: 
                //      Each program*catalog may have multiple requirements from ACPR.ACAD.REQMTS
                //      Requirements may also show up individually as a course prereq
                //      Requirements may also show up for each additional major/minor/etc in a StudentProgram

                #region ACAD.REQMTS
                // Colleague ACAD.REQMTS == Requirement

                Collection<AcadReqmts> rqs = new Collection<AcadReqmts>();

                // Execute the request, but only if a valid list of ids passed
                if ((requirementCodes != null) && (requirementCodes.Count() > 0))
                {
                    rqs = await DataReader.BulkReadRecordAsync<AcadReqmts>("ACAD.REQMTS", requirementCodes.ToArray());
                }

                // Build a dictionary of each requirement returned by the transaction request, indexed by block ID
                // so when we read the blocks later they're easy to find.

                Dictionary<string, Requirement> topBlocks = new Dictionary<string, Requirement>();

                if (rqs != null)
                {
                    char _VM = Convert.ToChar(DynamicArray.VM);
                    foreach (var rq in rqs)
                    {

                        string gradescheme = rq.AcrGradeScheme;
                        var reqType = await GetRequirementTypeAsync(rq.AcrType);

                        Requirement r = new Requirement(rq.AcrTopReqmtBlock, rq.Recordkey, rq.AcrDesc, rq.AcrGradeScheme, reqType);

                        // This should only exist at this level for prerequisites.  The "printed spec" from the CREQ form
                        // is stored here.  
                        if (!string.IsNullOrEmpty(rq.AcrPrintedSpec))
                        {
                            r.DisplayText = rq.AcrPrintedSpec;
                            r.DisplayText = r.DisplayText.Replace("" + _VM + _VM, Environment.NewLine + Environment.NewLine + "");
                            //If there is a single-VM, replace it with a space.
                            r.DisplayText = r.DisplayText.Replace(_VM, ' ');
                        }

                        if (reqs.Contains(r))
                        {
                            r = reqs.First(rr => rr.Id == r.Id);
                        }
                        else
                        {
                            reqs.Add(r);
                        }

                        topBlocks[rq.AcrTopReqmtBlock] = r;
                    }
                }

                #endregion

                #region ACAD.REQMT.BLOCKS that are 1-to-1 with requirements

                Collection<AcadReqmtBlocks> topblocks = await DataReader.BulkReadRecordAsync<AcadReqmtBlocks>("ACAD.REQMT.BLOCKS", topBlocks.Keys.Distinct().ToArray());

                List<Subrequirement> Subrequirements = new List<Subrequirement>();

                if (topblocks != null)
                {
                    foreach (var reqBlock in topblocks)
                    {
                        string topLevelBlockId = reqBlock.Recordkey;
                        Requirement r = topBlocks[topLevelBlockId];
                        r.MinSubRequirements = reqBlock.AcrbMinNoSubblocks;
                        r.Exclusions = reqBlock.AcrbExclReqmtTypes;
                        r.ExtraCourseDirective = (await GetDegreeAuditParametersAsync()).ExtraCourseHandling;

                        await AddSVToBlockBaseAsync(r, reqBlock, grades, printTextAsEntered);
                        AddSVToBlock(r, reqBlock);

                        // Requirement Include Low Grades setting. This is in BlockBase but must be set here because
                        // its default differs when it is a requirement vs subrequirement or group.
                        // Set to True unless Colleague field is set to "N". If blank, use the value from degree audit parameters.
                        r.IncludeLowGradesInGpa = string.IsNullOrEmpty(reqBlock.AcrbIncludeFailures) ? (await GetDegreeAuditParametersAsync()).UseLowGrade : reqBlock.AcrbIncludeFailures.ToUpper() != "N";

                        foreach (var subBlock in reqBlock.AcrbSubblocks)
                        {
                            string SubrequirementId = subBlock;
                            Subrequirement sr = new Subrequirement(SubrequirementId, "Placeholder");
                            topBlocks[topLevelBlockId].SubRequirements.Add(sr);
                            Subrequirements.Add(sr);
                        }
                    }
                }

                #endregion

                #region ACAD.REQMT.BLOCKS that are Subrequirements

                Collection<AcadReqmtBlocks> subblocks = await DataReader.BulkReadRecordAsync<AcadReqmtBlocks>("ACAD.REQMT.BLOCKS",
                                                                                                     Subrequirements.Select(sr => sr.Id).Distinct().ToArray(), false);
                List<Group> groups = new List<Group>();

                if (subblocks != null)
                {
                    foreach (var SubrequirementBlock in subblocks)
                    {
                        Subrequirement sr = Subrequirements.Find(s => s.Id == SubrequirementBlock.Recordkey);
                        if (sr != null)
                        {
                            sr.ExtraCourseDirective=topBlocks[SubrequirementBlock.AcrbParentBlock].ExtraCourseDirective;
                            await AddSVToBlockBaseAsync(sr, SubrequirementBlock, grades, printTextAsEntered );
                            AddSVToBlock(sr, SubrequirementBlock);
                            //Give the Subrequirement a reference to its parent
                            Requirement requirement = topBlocks[SubrequirementBlock.AcrbParentBlock];
                            sr.Requirement = requirement;

                            // Subrequirement Include Low Grades setting. This is in BlockBase but must be set here because
                            // its default differs when it is a requirement vs subrequirement or group.
                            // Set to True unless Colleague field is set to "N". If blank, get value from requirement
                            sr.IncludeLowGradesInGpa = string.IsNullOrEmpty(SubrequirementBlock.AcrbIncludeFailures) ? sr.Requirement.IncludeLowGradesInGpa : SubrequirementBlock.AcrbIncludeFailures.ToUpper() != "N";

                            sr.Code = SubrequirementBlock.AcrbLabel;

                            sr.MinGroups = SubrequirementBlock.AcrbMinNoSubblocks;

                            sr.SortSpecificationId = SubrequirementBlock.AcrbSortMethod;
                        }


                        foreach (var groupBlock in SubrequirementBlock.AcrbSubblocks)
                        {
                            string SubrequirementId = SubrequirementBlock.Recordkey;
                            string groupId = groupBlock;

                            //Give the group a reference to its parent
                            Subrequirement Subrequirement = Subrequirements.Find(s => s.Id == SubrequirementId);
                            Group g = new Group(groupId, "Placeholder", Subrequirement);
                            g.SortSpecificationId = !string.IsNullOrEmpty(Subrequirement.SortSpecificationId) ? Subrequirement.SortSpecificationId : Subrequirement.Requirement.SortSpecificationId;

                            groups.Add(g);
                            sr.Groups.Add(g);
                        }
                    }
                }

                #endregion

                #region ACAD.REQMT.BLOCKS that are groups

                Collection<AcadReqmtBlocks> groupblocks = await DataReader.BulkReadRecordAsync<AcadReqmtBlocks>("ACAD.REQMT.BLOCKS",
                                                                                                      groups.Select(g => g.Id).Distinct().ToArray());
                var foo = groups.Select(g => g.Id).Distinct().ToArray();

                Dictionary<Group, int> maxCoursesAtLevelsMap = new Dictionary<Group, int>();
                Dictionary<Group, decimal> maxCreditsAtLevelsMap = new Dictionary<Group, decimal>();
                if (groupblocks != null)
                {
                    foreach (var groupBlock in groupblocks)
                    {
                        Group group = groups.Find(g => g.Id == groupBlock.Recordkey);
                        if (group != null)
                        {
                            group.SortSpecificationId = !string.IsNullOrEmpty(groupBlock.AcrbSortMethod) ? groupBlock.AcrbSortMethod : group.SortSpecificationId;
                            group.ExtraCourseDirective = group.SubRequirement.ExtraCourseDirective;
                            await AddSVToBlockBaseAsync(group, groupBlock, grades, printTextAsEntered);

                            // Group Include Low Grades setting. This is in BlockBase but must be set here because
                            // its default differs when it is a requirement vs subrequirement or group.
                            // Set to True unless Colleague field is set to "N". If blank, get value from its subrequirement
                            group.IncludeLowGradesInGpa = string.IsNullOrEmpty(groupBlock.AcrbIncludeFailures) ? group.SubRequirement.IncludeLowGradesInGpa : groupBlock.AcrbIncludeFailures.ToUpper() != "N";

                            // Get the "type" so the groups can be sorted in the order of this enum, same as in old EVAL
                            GroupType gt;
                            switch (groupBlock.AcrbType)
                            {
                                case "30": { gt = GroupType.TakeAll; break; }
                                case "31": { gt = GroupType.TakeSelected; break; }
                                case "32": { gt = GroupType.TakeCredits; break; }
                                case "33": { gt = GroupType.TakeCourses; break; }
                                case "34": throw new NotSupportedException("CUSTOM.MATCH not supported"); //custom match
                                default: gt = GroupType.TakeCourses; break;/*throw new NotSupportedException("Unknown group type: " + group.InternalType)*/;
                            }
                            group.GroupType = gt;

                            group.Code = groupBlock.AcrbLabel;
                            group.MaxCoursesPerDepartment = groupBlock.AcrbMaxCoursesPerDept;
                            group.MaxCoursesPerRule = groupBlock.AcrbMaxCoursesPerRule;
                            if (!string.IsNullOrEmpty(groupBlock.AcrbMaxCoursesRules))
                            {
                                var reqRule = RequirementRule.TryCreate(await ruleRepository.GetAsync(groupBlock.AcrbMaxCoursesRules));
                                if (reqRule == null)
                                {
                                    if (logger.IsInfoEnabled)
                                    {
                                        logger.Info("Rule " + groupBlock.AcrbMaxCoursesRules + " on group " + group.Id + " ignored");
                                    }
                                }
                                else
                                {
                                    group.MaxCoursesRule = reqRule;
                                }
                            }

                            group.MaxCoursesPerSubject = groupBlock.AcrbMaxCoursesPerSubject;
                            group.MaxCredits = groupBlock.AcrbMaxCred;
                            group.MaxCreditsPerCourse = groupBlock.AcrbMaxCredPerCourse;
                            group.MaxCreditsPerSubject = groupBlock.AcrbMaxCredPerSubject;
                            group.MaxCreditsPerRule = groupBlock.AcrbMaxCredPerRule;
                            group.MaxCreditsPerDepartment = groupBlock.AcrbMaxCredPerDept;
                            if (!string.IsNullOrEmpty(groupBlock.AcrbMaxCredRules))
                            {
                                var reqRule = RequirementRule.TryCreate(await ruleRepository.GetAsync(groupBlock.AcrbMaxCredRules));
                                if (reqRule == null)
                                {
                                    if (logger.IsInfoEnabled)
                                    {
                                        logger.Info("Rule " + groupBlock.AcrbMaxCredRules + " on group " + group.Id + " ignored");
                                    }
                                }
                                else
                                {
                                    group.MaxCreditsRule = reqRule;
                                }
                            }
                            group.MaxCourses = groupBlock.AcrbMaxNoCourses;
                            group.MaxDepartments = groupBlock.AcrbMaxNoDepts;
                            group.MaxSubjects = groupBlock.AcrbMaxNoSubjects;

                            group.MinCoursesPerDepartment = groupBlock.AcrbMinCoursesPerDept;
                            group.MinCoursesPerSubject = groupBlock.AcrbMinCoursesPerSubject;
                            group.MinCredits = groupBlock.AcrbMinCred;
                            group.MinCreditsPerCourse = groupBlock.AcrbMinCredPerCourse;
                            group.MinCreditsPerDepartment = groupBlock.AcrbMinCredPerDept;
                            group.MinCreditsPerSubject = groupBlock.AcrbMinCredPerSubject;
                            group.MinCourses = groupBlock.AcrbMinNoCourses;
                            group.MinDepartments = groupBlock.AcrbMinNoDepts;
                            group.MinSubjects = groupBlock.AcrbMinNoSubjects;

                            int? maxCoursesAtLevels = groupBlock.AcrbNoLevelNoCourses;
                            if (maxCoursesAtLevels.HasValue)
                            {
                                ICollection<string> levelList = groupBlock.AcrbNoLevelCoursesLevels;
                                group.MaxCoursesAtLevels = new MaxCoursesAtLevels((Int32)maxCoursesAtLevels, levelList);
                            }

                            decimal? maxCreditsAtLevels = groupBlock.AcrbNoLevelCred;
                            if (maxCreditsAtLevels.HasValue)
                            {
                                ICollection<string> levelList = groupBlock.AcrbNoLevelCredLevels;
                                group.MaxCreditsAtLevels = new MaxCreditAtLevels((Decimal)maxCreditsAtLevels, levelList);
                            }

                            foreach (var butNotCourses in groupBlock.AcrbButNotCourses)
                            {
                                if (butNotCourses != "") { group.ButNotCourses.Add(butNotCourses); }
                            }

                            foreach (var butNotDepts in groupBlock.AcrbButNotDepts)
                            {
                                if (butNotDepts != "") { group.ButNotDepartments.Add(butNotDepts); }
                            }

                            foreach (var butNotLevels in groupBlock.AcrbButNotCrsLevels)
                            {
                                if (butNotLevels != "") { group.ButNotCourseLevels.Add(butNotLevels); }
                            }

                            if (groupBlock.AcrbButNotSubjects != null)  //no idea why this one can be null
                            {
                                foreach (var butNotSubjects in groupBlock.AcrbButNotSubjects)
                                {
                                    if (butNotSubjects != "") { group.ButNotSubjects.Add(butNotSubjects); }
                                }
                            }

                            foreach (var courses in groupBlock.AcrbCourses)
                            {
                                if (courses != "") { group.Courses.Add(courses); }
                            }

                            foreach (var fromCourses in groupBlock.AcrbFromCourses)
                            {
                                if (fromCourses != "") { group.FromCourses.Add(fromCourses); }
                            }

                            foreach (var fromSubjects in groupBlock.AcrbFromSubjects)
                            {
                                if (fromSubjects != "") { group.FromSubjects.Add(fromSubjects); }
                            }

                            foreach (var fromDepts in groupBlock.AcrbFromDepts)
                            {
                                if (fromDepts != "") { group.FromDepartments.Add(fromDepts); }
                            }

                            foreach (var fromLevel in groupBlock.AcrbFromCrsLevels)
                            {
                                if (fromLevel != "") { group.FromLevels.Add(fromLevel); }
                            }

                            group.Exclusions = groupBlock.AcrbExclReqmtTypes;
                        }
                    }
                }

                #endregion
            }

            foreach (var req in reqs)
            {
                // Cascade min grade and allowed grades down to subrequirements and groups
                CascadeInheritedGrades(req);
                // Cascade requirement sort spec to subreqs and their groups if necessary
                req.CascadeSortSpecificationWhenNecessary();
                // Update cache with the requirement
                _cacheProvider.Remove(req.Code);
                GetOrAddToCache<Requirement>(req.Code, () =>
                {
                    return req;
                });
            }
            return reqs;
        }


        /// <summary>
        ///will read through WAPP Default values to check if "my progress" in Self service is similar to html version of EVAL or
        ///is shown as it is.
        /// </summary>
        /// <returns>bool</returns>
        private async Task<bool> RetrievePrintTextParamAsync()
        {
            Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = await GetStwebDefaultsAsync();
            return ( !string.IsNullOrEmpty(stwebDefaults.StwebFormatProgPrintText) && stwebDefaults.StwebFormatProgPrintText.Equals("Y",StringComparison.OrdinalIgnoreCase)) ? true:false;
        }


        private async Task AddSVToBlockBaseAsync(BlockBase r, AcadReqmtBlocks block, IEnumerable<Grade> grades, bool printTextAsEntered)
        {
            // For prerequisites this can already be populated and should not be overwritten
            if (string.IsNullOrEmpty(r.DisplayText))
            {
                r.DisplayText = block.AcrbPrintedSpec == null ? "" : block.AcrbPrintedSpec;
                if (r.DisplayText != string.Empty)
                {
                    r.DisplayText = ReplaceSpecialPrintChars(r.DisplayText, printTextAsEntered);
                }
            }
            if(!string.IsNullOrEmpty(block.AcrbExtraCode))
            {
                switch (block.AcrbExtraCode)
                {
                    case _IgnoreCode:
                        r.ExtraCourseDirective = ExtraCourses.Ignore;
                        break;
                    case _DisplayCode:
                        r.ExtraCourseDirective = ExtraCourses.Display;
                        break;
                    case _SemiApplyCode:
                        r.ExtraCourseDirective = ExtraCourses.SemiApply;
                        break;
                    case _ApplyCode:
                        r.ExtraCourseDirective = ExtraCourses.Apply;
                        break;
                    default:
                        r.ExtraCourseDirective = ExtraCourses.Apply;
                        break;
                }
            }


            if (block.AcrbAcadCredRules != null)
            {
                var ruleIds = block.AcrbAcadCredRules.Where(rid => !string.IsNullOrEmpty(rid));
                var rules = await ruleRepository.GetManyAsync(ruleIds);
                foreach (var rule in rules)
                {
                    var requirementRule = RequirementRule.TryCreate(rule);
                    if (requirementRule == null)
                    {
                        if (logger.IsInfoEnabled)
                        {
                            logger.Info("Rule " + rule.Id + " ignored on block " + block.Recordkey);
                        }
                    }
                    else
                    {
                        r.AcademicCreditRules.Add(requirementRule);
                    }
                }
            }
            r.MinInstitutionalCredits = block.AcrbInstitutionCred;
            r.MinGpa = block.AcrbMinGpa;

            // this is a key to DA.SORT.SPECS
            if (r.GetType() != typeof(Group))
            {
                string sortMethod = block.AcrbSortMethod;
                r.SortSpecificationId = sortMethod;
            }


            // Min Grade
            if (!string.IsNullOrEmpty(block.AcrbMinGrade))
            {
                try
                {
                    r.MinGrade = grades.First(g => g.Id == block.AcrbMinGrade);
                }
                catch
                {
                    if (logger.IsInfoEnabled)
                    {
                        logger.Info("Invalid Minimum Grade of " + block.AcrbMinGrade + " defined on ACAD.REQMTS.BLOCK " + block.Recordkey);
                    }
                }
            }

            // Allowed Grades
            if (block.AcrbAllowedGrades != null && block.AcrbAllowedGrades.Count() > 0)
            {
                foreach (var allowedGrade in block.AcrbAllowedGrades)
                {
                    if (!string.IsNullOrEmpty(allowedGrade))
                    {
                        try
                        {
                            var grd = grades.First(g => g.Id == allowedGrade);
                            r.AllowedGrades.Add(grd);
                        }
                        catch
                        {
                            if (logger.IsInfoEnabled)
                            {
                                logger.Info("Invalid Allowed Grade of " + allowedGrade + " defined on ACAD.REQMTS.BLOCK " + block.Recordkey);
                            }
                        }
                    }
                }
            }


            r.InternalType = block.AcrbType;
        }

        private string ReplaceSpecialPrintChars(string displayText, bool printTextAsEntered)
        {
            char _VM = Convert.ToChar(DynamicArray.VM);
            string replacedString = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(displayText))
                {
                    replacedString = displayText;
                    //if printTextAsEntered is false - convert similar to html report of EVAL otherwise show as is.
                    if (!printTextAsEntered)
                    {
                        // If there is a  #, replace them with new line. 
                        replacedString = System.Text.RegularExpressions.Regex.Replace(replacedString, "#", "\n" );
                        // If there is are multiple VMs, replace them with NewLines (so they get treated as "paragraphs").
                        replacedString = System.Text.RegularExpressions.Regex.Replace(replacedString, _VM + "{2,}", Environment.NewLine+Environment.NewLine);
                        // If there is a single-VM, replace it with a space.
                        replacedString = System.Text.RegularExpressions.Regex.Replace(replacedString, _VM + "{1}", " ");
                    }
                    else
                    {
                        // If there is a double-VM, replace them with NewLines (so they get treated as "paragraphs")
                        replacedString = replacedString.Replace("" + _VM + _VM, Environment.NewLine + Environment.NewLine + "");
                        // If there is a single-VM, replace it with a new line.
                        replacedString = replacedString.Replace(_VM, '\n');
                    }
                }
            }
            catch
            {
                throw;
            }
            return replacedString;
        }


        private void AddSVToBlock(RequirementBlock r, AcadReqmtBlocks block)
        {
            r.AllowsCourseReuse = block.AcrbCourseReuseFlag != "N";
            r.WaitToMerge = block.AcrbMergeMethod == "Y";
        }

        public async Task<DegreeAuditParameters> GetDegreeAuditParametersAsync()
        {

            // Overriding cache timeout to be Level1 Cache time out for data that rarely changes.
            var degreeAuditParameters = await GetOrAddToCacheAsync<DegreeAuditParameters>("DegreeAuditDefaults",
                   async () =>
                   {
                       Data.Student.DataContracts.DaDefaults daDefaults = await DataReader.ReadRecordAsync<Data.Student.DataContracts.DaDefaults>("ST.PARMS", "DA.DEFAULTS");

                       if (daDefaults != null)
                       {
                           ExtraCourses extraCourses = ExtraCourses.Apply;
                           if (!string.IsNullOrEmpty(daDefaults.DaExtraCode))
                           {
                               switch (daDefaults.DaExtraCode.ToUpper())
                               {
                                   case _IgnoreCode:
                                       extraCourses = ExtraCourses.Ignore;
                                       break;
                                   case _DisplayCode:
                                       extraCourses = ExtraCourses.Display;
                                       break;
                                   case _SemiApplyCode:
                                       extraCourses = ExtraCourses.SemiApply;
                                       break;
                                   case _ApplyCode:
                                       extraCourses = ExtraCourses.Apply;
                                       break;
                                   default:
                                       extraCourses = ExtraCourses.Apply;
                                       break;
                               }
                           }
                           // UseLowGrade is true unless it is specifically N - so null, empty and Y are all true.
                           return new DegreeAuditParameters(extraCourses,
                               !string.IsNullOrEmpty(daDefaults.DaIncludeFailures) && daDefaults.DaIncludeFailures.ToUpper() == "N" ? false : true,
                               !string.IsNullOrEmpty(daDefaults.DaDefaultSortOverride) && daDefaults.DaDefaultSortOverride.ToUpper() == "Y" ? true : false);
                       }
                       else
                       {
                           // Create parameters using the defaults
                           return new DegreeAuditParameters(ExtraCourses.Apply, true, false);
                       }
                   }, Level1CacheTimeoutValue);

            return degreeAuditParameters;


        }

        /// <summary>
        /// Make sure that any min grade and allowed grades defined in the program requirements are inherited
        /// by the requirements, subrequirements and groups where they are empty. Never overwrite existing settings
        /// and stop cascading when a min grade is found in the level below.
        /// </summary>
        /// <param name="requirement">Requirement object to update</param>
        /// <param name="programRequirements">Program Requirements object holding this requirement</param>
        private static void CascadeInheritedGrades(Requirement requirement)
        {
            foreach (var subReq in requirement.SubRequirements)
            {
                // If there is no min grade defined in this subrequirement, inherit the min grade and allowed grades
                // from the requirement. 
                if (subReq.MinGrade == null && requirement.MinGrade != null)
                {
                    subReq.MinGrade = requirement.MinGrade;
                    subReq.AllowedGrades = requirement.AllowedGrades;
                }
                foreach (var group in subReq.Groups)
                {
                    // If there is no min grade defined in this group, inherit min grade and allowed grades
                    // from the subrequirement.
                    if (group.MinGrade == null && subReq.MinGrade != null)
                    {
                        group.MinGrade = subReq.MinGrade;
                        group.AllowedGrades = subReq.AllowedGrades;
                    }
                }
            }
        }


        private async Task<RequirementType> GetRequirementTypeAsync(string reqTypeCode)
        {
            RequirementType requirementType = null;
            try
            {
                // Get the list of requirement types from cache (or from Colleague if first time)
                var requirementTypes = await GetValcodeAsync<RequirementType>("ST", "ACAD.REQMT.TYPES", r => new RequirementType(r.ValInternalCodeAssocMember, r.ValExternalRepresentationAssocMember, r.ValActionCode1AssocMember));
                // If a requirement type code is provided, find it in the validation table
                if (!string.IsNullOrEmpty(reqTypeCode))
                {
                    requirementType = requirementTypes.Where(r => r.Code == reqTypeCode).FirstOrDefault();
                    // If a requirement code is not found in the validation table, Log the error and create a default "unknown" 
                    // item with no priority--which will default to the highest sequence number and last priority.
                    if (requirementType == null)
                    {
                        requirementType = new RequirementType(reqTypeCode, "Unknown requirement type", null);
                        if (logger.IsInfoEnabled)
                        {
                            logger.Info("Requirement Type code " + reqTypeCode + " is not found in ACAD.REQMT.TYPES. Defaulting to lowest priority.");
                        }
                    }
                }
                else
                {
                    // if the requirement type code is null or empty, it is likely a requisite therefore requirement type is
                    // irrelevant. Return a "NONE" requirement type.
                    requirementType = new RequirementType("NONE", "Empty Requirement Type", null);
                }
            }
            catch (Exception ex)
            {
                if (logger.IsErrorEnabled)
                {
                    logger.Error(ex.Message);
                    logger.Info("Error occurred when attempting to process valcode ACAD.REQMT.TYPES");
                    throw ex;
                }
            }
            return requirementType;
        }

        private async Task<Data.Student.DataContracts.StwebDefaults> GetStwebDefaultsAsync()
        {
            Data.Student.DataContracts.StwebDefaults studentWebDefaults = await GetOrAddToCacheAsync<Data.Student.DataContracts.StwebDefaults>("StudentWebDefaults",
            async () =>
            {
                Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", false);
                if (stwebDefaults == null)
                {
                    var errorMessage = "Unable to access student web defaults from ST.PARMS. STWEB.DEFAULTS.";
                    logger.Info(errorMessage);
                    stwebDefaults = new StwebDefaults();
                }
                return stwebDefaults;
            }, Level1CacheTimeoutValue);
            return studentWebDefaults;
        }
    }
}
