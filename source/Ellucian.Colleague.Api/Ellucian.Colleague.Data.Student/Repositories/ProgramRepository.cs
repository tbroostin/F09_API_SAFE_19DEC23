// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
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
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class ProgramRepository : BaseColleagueRepository, IProgramRepository
    {
        private IStudentReferenceDataRepository studentReferenceRepo;

        public ProgramRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using Level 1 time out for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
            studentReferenceRepo = new StudentReferenceDataRepository(cacheProvider, transactionFactory, logger, apiSettings);
        }

        public async Task<IEnumerable<Program>> GetAsync()
        {
            try
            {
                var transGroupings = await GetOrAddToCacheAsync<Collection<TranscriptGroupings>>("AllTranscriptGroupings",
                    async () =>
                    {
                        var transGroups = await DataReader.BulkReadRecordAsync<TranscriptGroupings>("TRANSCRIPT.GROUPINGS", "");
                        return transGroups;
                    }
                , Level1CacheTimeoutValue);

                //TODO: when method should return null object - but then fails as async
                var creditTypes = await GetOrAddToCacheAsync<Collection<CredTypes>>("AllCreditTypes",
                   async () =>
                    {
                        var credTypes = await DataReader.BulkReadRecordAsync<CredTypes>("CRED.TYPES", "");
                        return credTypes;
                    }
                , Level1CacheTimeoutValue);

                var programData = await GetOrAddToCacheAsync<List<Program>>("AllPrograms",
                    async () =>
                    {
                        var acadPrograms = await DataReader.BulkReadRecordAsync<AcadPrograms>("ACAD.PROGRAMS", "");
                        var programList = await BuildProgramsAsync(acadPrograms, transGroupings, creditTypes);
                        return programList;
                    }
                );
                return programData;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while retrieving programs");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception occurred while retrieving programs");
                throw;
            }
        }

        public async Task<Program> GetAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("Program code must be specified");
            }
            try
            {
                return (await GetAsync()).Where(p => p.Code == code).First();
            }
            catch
            {
                throw new ArgumentOutOfRangeException("No Program found for Code" + code);
            }
        }

        public async Task<IEnumerable<StudentPrograms>> GetAsync(List<string> programIds)
        {
            return await DataReader.BulkReadRecordAsync<StudentPrograms>("STUDENT.PROGRAMS", programIds.ToArray());
        }

        public async Task<StudentProgram> GetAsync(string id, string programCode)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("Student id must be specified");
            }
            if (string.IsNullOrEmpty(programCode))
            {
                throw new ArgumentNullException("Program code must be specified");
            }
            try
            {
                var stprIds = new List<string>();
                stprIds.Add(id + "*" + programCode);

                var ProgramData = (await GetAsync(stprIds)).First();

                return new StudentProgram(id, programCode, ProgramData.StprCatalog)
                {
                    AnticipatedCompletionDate = ProgramData.StprAntCmplDate
                };
            }
            catch
            {
                throw new ArgumentOutOfRangeException("No Program found for Code" + programCode);
            }
        }

        private async Task<List<Program>> BuildProgramsAsync(Collection<AcadPrograms> programData, IEnumerable<TranscriptGroupings> transGroupData, IEnumerable<CredTypes> credTypeData)
        {
            var programs = new List<Program>();
            // If no data passed in, return a null collection
            if (programData != null)
            {
                var programStatuses = await GetProgramStatusesAsync();
                foreach (var prog in programData)
                {
                    try
                    {
                        if (prog.AcpgDesc != null)
                        {
                            // If there is a double-VM, replace them with NewLines (so they get treated as "paragraphs")
                            prog.AcpgDesc = prog.AcpgDesc.Replace("" + DmiString._VM + DmiString._VM, Environment.NewLine + Environment.NewLine + "");
                            // If there is a single-VM, replace it with a space.
                            prog.AcpgDesc = prog.AcpgDesc.Replace(DmiString._VM, ' ');
                        }
                        // Get the first item from the list of statuses and retrieve the corresponding entry from the valcode association
                        ApplValcodesVals codeItem = null;
                        try
                        {
                            codeItem = programStatuses.ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == prog.ProgramStatusEntityAssociation.ElementAt(0).AcpgStatusAssocMember).FirstOrDefault();
                        }
                        catch (Exception)
                        {
                            // status code bad or missing
                            logger.Info("Data error: status code for program " + prog.Recordkey + " bad or missing.");
                        }

                        // Program is inactive if it has an action code of 2 AND an end date prior to today
                        bool isActive = true;
                        if ((codeItem != null) && (codeItem.ValActionCode1AssocMember == "2" && prog.AcpgEndDate <= DateTime.Today))
                        {
                            isActive = false;
                        }
                        bool isGraduationAllowed = prog.AcpgAllowGraduationFlag.ToUpper() == "Y";
                        // Get the transcript grouping and create a CreditFilter object for the Program.  It is theoretically
                        // impossible to not have one.  However, it could happen that there is bad data. 
                        CreditFilter creditFilter = new CreditFilter();
                        TranscriptGroupings trgrp = null;
                        try
                        {
                            trgrp = transGroupData.FirstOrDefault(trgr => trgr.Recordkey == prog.AcpgTranscriptGrouping);
                        }
                        catch (Exception)
                        {
                            // Transcript grouping data bad or missing.
                            logger.Info("Data error: transcript grouping " + prog.AcpgTranscriptGrouping + " bad or missing.");
                        }

                        if (trgrp != null)
                        {
                            if (trgrp.TrgpAcadLevels != null)
                            {
                                creditFilter.AcademicLevels.AddRange(trgrp.TrgpAcadLevels);
                            }
                            else
                            {
                                if (logger != null)
                                {
                                    logger.Info("Data error: transcript grouping " + prog.AcpgTranscriptGrouping + " missing academic level");
                                }
                            }

                            if (trgrp.TrgpCourseLevels != null && trgrp.TrgpCourseLevels.Count > 0)
                            {
                                foreach (var courselevel in trgrp.TrgpCourseLevels)
                                {
                                    if (!string.IsNullOrEmpty(courselevel)) { creditFilter.CourseLevels.Add(courselevel); }
                                }

                            }
                            if (trgrp.TrgpAcadCredMarks != null && trgrp.TrgpAcadCredMarks.Count > 0)
                            {
                                foreach (var mark in trgrp.TrgpAcadCredMarks)
                                {
                                    if (!string.IsNullOrEmpty(mark)) { creditFilter.Marks.Add(mark); }
                                }
                            }
                            if (trgrp.TrgpCredTypes != null && trgrp.TrgpCredTypes.Count > 0)
                            {
                                foreach (var credtype in trgrp.TrgpCredTypes)
                                {
                                    if (!string.IsNullOrEmpty(credtype))
                                    {
                                        creditFilter.CreditTypes.Add(credtype);
                                    }
                                }
                            }

                            if (trgrp.TrgpDepts != null && trgrp.TrgpDepts.Count > 0)
                            {
                                foreach (var dept in trgrp.TrgpDepts)
                                {
                                    if (!string.IsNullOrEmpty(dept)) { creditFilter.Departments.Add(dept); }
                                }

                            }

                            if (trgrp.TrgpSubjects != null && trgrp.TrgpSubjects.Count > 0)
                            {
                                foreach (var subj in trgrp.TrgpSubjects)
                                {
                                    if (!string.IsNullOrEmpty(subj)) { creditFilter.Subjects.Add(subj); }
                                }

                            }
                            //add schools, locations, divisions  to creditFilter
                            if (trgrp.TrgpDivisions != null && trgrp.TrgpDivisions.Count > 0)
                            {
                                creditFilter.Divisions.AddRange(trgrp.TrgpDivisions.Where(t => !string.IsNullOrEmpty(t)));
                            }
                            if (trgrp.TrgpSchools != null && trgrp.TrgpSchools.Count > 0)
                            {
                                creditFilter.Schools.AddRange(trgrp.TrgpSchools.Where(t => !string.IsNullOrEmpty(t)));
                            }
                            if (trgrp.TrgpLocations != null && trgrp.TrgpLocations.Count > 0)
                            {
                                creditFilter.Locations.AddRange(trgrp.TrgpLocations.Where(t => !string.IsNullOrEmpty(t)));
                            }


                            // Credit filter IncludeNeverGradedCredits defaults to true because the Colleague transcript grouping form
                            // defaults Incl No Grade to Yes.  If the transcript grouping has TrgpInclNoGradesFlag not Y then set it to false.
                            if (trgrp.TrgpInclNoGradesFlag != "Y") { creditFilter.IncludeNeverGradedCredits = false; }

                            //add additional selection criteria from transcript grouping to credit filter
                            creditFilter.AdditionalSelectCriteria = trgrp.TrgpAddnlSelectCriteria;
                        }
                        else
                        {
                            if (logger != null)
                            {
                                logger.Info("Data error: transcript grouping not found: " + prog.AcpgTranscriptGrouping);
                            }
                        }

                        Program program = new Program(prog.Recordkey, prog.AcpgTitle, prog.AcpgDepts, isActive, prog.AcpgAcadLevel, creditFilter, isGraduationAllowed, prog.AcpgDesc, prog.AcpgLocations);
                        program.MonthsToComplete = prog.AcpgCmplMonths;
                        program.Catalogs = prog.AcpgCatalogs;
                        program.Degree = "";
                        program.TranscriptGrouping = prog.AcpgTranscriptGrouping;
                        program.UnofficialTranscriptGrouping = prog.AcpgUnoffTransGrouping;
                        program.OfficialTranscriptGrouping = prog.AcpgOfficialTransGrouping;
                        program.ProgramStartDate = prog.AcpgStartDate;
                        program.ProgramEndDate = prog.AcpgEndDate;

                        if (!string.IsNullOrEmpty(prog.AcpgDegree))
                        {
                            var deg = (await studentReferenceRepo.GetDegreesAsync()).FirstOrDefault(dg => dg.Code == prog.AcpgDegree);
                            if (deg != null)
                            {
                                program.Degree = deg.Description;
                            }
                        }

                        if ((prog.AcpgMajors != null) && !(prog.AcpgMajors.Count() == 1 && prog.AcpgMajors.ElementAt(0) == ""))
                        {
                            List<string> majors = new List<string>();
                            foreach (var majorcode in prog.AcpgMajors)
                            {
                                var major = (await studentReferenceRepo.GetMajorsAsync()).FirstOrDefault(maj => maj.Code == majorcode);
                                if (major != null)
                                {
                                    string awardName = major.Description;
                                    if (awardName != null) { majors.Add(awardName); }
                                }
                            }
                            program.Majors = majors;
                        }

                        if ((prog.AcpgMinors != null) && !(prog.AcpgMinors.Count() == 1 && prog.AcpgMinors.ElementAt(0) == ""))
                        {
                            List<string> minors = new List<string>();
                            foreach (var minorcode in prog.AcpgMinors)
                            {
                                var minor = (await studentReferenceRepo.GetMinorsAsync()).FirstOrDefault(min => min.Code == minorcode);
                                if (minor != null)
                                {
                                    string awardName = minor.Description;
                                    if (awardName != null) { minors.Add(awardName); }
                                }
                            }
                            program.Minors = minors;
                        }

                        if ((prog.AcpgCcds != null) && !(prog.AcpgCcds.Count() == 1 && prog.AcpgCcds.ElementAt(0) == ""))
                        {
                            List<string> ccds = new List<string>();
                            foreach (var ccdcode in prog.AcpgCcds)
                            {
                                var ccd = (await studentReferenceRepo.GetCcdsAsync()).FirstOrDefault(cd => cd.Code == ccdcode);
                                if (ccd != null)
                                {
                                    string awardName = ccd.Description;
                                    if (awardName != null) { ccds.Add(awardName); }
                                }
                            }
                            program.Ccds = ccds;
                        }

                        if ((prog.AcpgSpecializations != null) && !(prog.AcpgSpecializations.Count() == 1 && prog.AcpgSpecializations.ElementAt(0) == ""))
                        {
                            List<string> spcs = new List<string>();
                            foreach (var spccode in prog.AcpgSpecializations)
                            {
                                var spc = (await studentReferenceRepo.GetSpecializationsAsync()).FirstOrDefault(sp => sp.Code == spccode);
                                if (spc != null)
                                {
                                    string awardName = spc.Description;
                                    if (awardName != null) { spcs.Add(awardName); }
                                }
                            }
                            program.Specializations = spcs;

                        }

                        // Update boolean indicating whether program should be available for student selection
                        program.IsSelectable = prog.AcpgStudentSelectFlag == "Y" ? true : false;

                        // List of programs to use in what-if fastest path analysis
                        program.RelatedPrograms = prog.AcpgRelatedPrograms;

                        programs.Add(program);

                    }
                    catch (ColleagueSessionExpiredException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        LogDataError("Program", prog.Recordkey, prog, ex);
                        //throw new ArgumentException("Error occurred when trying to build program " + prog.Recordkey);
                    }
                }
            }
            return programs;
        }

        /// <summary>
        /// Get Program Statuses using the PROGRAM.STATUSES valcode table 
        /// </summary>
        /// <returns>ApplValcodes<string, ProgramStatus></returns>
        private async Task<ApplValcodes> GetProgramStatusesAsync()
        {
            // Overriding cache timeout to be Level1 Cache time out for data that rarely changes.
            var programStatuses = await GetOrAddToCacheAsync<ApplValcodes>("ProgramStatuses",
                async () =>
                {
                    ApplValcodes programStatusesValcode = await DataReader.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "PROGRAM.STATUSES");

                    if (programStatusesValcode == null)
                    {
                        var errorMessage = "Unable to access PROGRAM.STATUSES valcode table.";
                        logger.Info(errorMessage);
                        throw new ColleagueWebApiException(errorMessage);
                    }
                    return programStatusesValcode;
                }, Level1CacheTimeoutValue);
            return programStatuses;
        }

    }
}
