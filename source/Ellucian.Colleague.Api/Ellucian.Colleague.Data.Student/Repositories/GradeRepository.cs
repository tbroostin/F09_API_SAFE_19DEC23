// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Utility;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class GradeRepository : BaseColleagueRepository, IGradeRepository
    {
        public GradeRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using Level 1 Cache Timeout Value for data that changes rarely.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        public async Task<ICollection<Grade>> GetAsync()
        {
            var grades = await GetOrAddToCacheAsync<List<Grade>>("AllGrades",
                async () =>
                {
                    Collection<Grades> gradesData = await DataReader.BulkReadRecordAsync<Grades>("");
                    // No need for a separate comparisonGradeData list, since gradesData contains all grade records
                    var gradeList = await BuildGradesAsync(gradesData, null);
                    return gradeList;
                }
            );
            return grades;
        }

        /// <summary>
        /// Convert a list of Grades data contract objects to a list of Grade entity objects
        /// </summary>
        /// <param name="gradeData"> The list of Grades objects to convert</param>
        /// <param name="comparisonGradeData">
        ///     An optional list of Grades objects of comparison grades specified within the gradeData list.
        ///     In some cases the gradeData list will contain all needed comparison grade objects, so this
        ///     list is not needed. The method will search for comparison grade objects first in gradeData, and
        ///     then in comparisonGradeData.
        /// </param>
        /// <returns></returns>

        private async Task<List<Grade>> BuildGradesAsync(Collection<Grades> gradeData, Collection<Grades> comparisonGradeData)
        {
            var grades = new List<Grade>();
            // If no data passed in, return a null collection
            if (gradeData != null)
            {
                foreach (var grd in gradeData)
                {
                    try
                    {
                        var includeInWebFinalGradesList = !string.IsNullOrEmpty(grd.GrdUseInFinalGrdList) && grd.GrdUseInFinalGrdList.ToUpper() == "Y";
                        var includeInWebMidtermGradesList = !string.IsNullOrEmpty(grd.GrdUseInMidtermGrdList) && grd.GrdUseInMidtermGrdList.ToUpper() == "Y";
                        var canBeUsedAfterDropGradeRequiredDate = !string.IsNullOrEmpty(grd.GrdUseAfterDropGrdReqd) && grd.GrdUseAfterDropGrdReqd.ToUpper() == "Y";
                        var grade = new Grade(grd.RecordGuid, grd.Recordkey, grd.GrdGrade, grd.GrdCmplCredFlag, grd.GrdLegend, grd.GrdGradeScheme, includeInWebFinalGradesList, includeInWebMidtermGradesList, canBeUsedAfterDropGradeRequiredDate);
                        grade.GradeValue = grd.GrdValue;//?? 0m;
                        grade.GradePriority = grd.GrdRepeatValue ?? 0m;
                        grade.IncompleteGrade = grd.GrdIncompleteGrade;
                        grade.ExcludeFromFacultyGrading = !string.IsNullOrEmpty(grd.GrdExcludeFromFacFlag) && grd.GrdExcludeFromFacFlag.ToUpper() == "Y";
                        grade.RequireLastAttendanceDate = !string.IsNullOrEmpty(grd.GrdFinalRequireLda) && grd.GrdFinalRequireLda.ToUpper() == "Y";

                        if (!string.IsNullOrEmpty(grd.GrdComparisonGrade)) {
                            // Find the comparison grade scheme in the main list, or the comparisonGradeData list
                            Grades comparisonGrade;
                            comparisonGrade = gradeData.Where(g => (g != null) && (g.Recordkey == grd.GrdComparisonGrade)).FirstOrDefault();
                            if (comparisonGrade == null) {
                                comparisonGrade = comparisonGradeData.Where(g => (g != null) && (g.Recordkey == grd.GrdComparisonGrade)).FirstOrDefault();
                            }

                            if (comparisonGrade == null)
                            {
                                logger.Error("Comparison Grade ID " + grd.GrdComparisonGrade + " is not a valid Grade entity");
                            } else
                            {
                                // Colleague erroneously allows the user to specify a comparison grade for a grade that is already in the comparison
                                // grade scheme. When this occurs, the EVAL Envision code ignores the comparison grade, so just don't populate the 
                                // comparison grade in the entity in this scenario.
                                if (grd.GrdGradeScheme == comparisonGrade.GrdGradeScheme)
                                {
                                    logger.Error("Grade ID " + grd.Recordkey + " and its comparison grade ID " + grd.GrdComparisonGrade + " are in the same grade scheme " + grd.GrdGradeScheme + ". Ignoring the comparison grade.");
                                } else
                                {
                                    grade.SetComparisonGrade(comparisonGrade.Recordkey, comparisonGrade.GrdValue, comparisonGrade.GrdGradeScheme);
                                }
                            }
                        }
                        grades.Add(grade);
                    }
                    catch (Exception ex)
                    {
                        LogDataError("Grade", grd.Recordkey, grd, ex);
                        //throw new ArgumentException("Error occurred when trying to build grade " + grd.Recordkey);
                    }

                }
                // Loop through all the grades used as withdraw grades and update the boolean in each respective grade object
                foreach (var repoGrade in gradeData.Where(g => !(string.IsNullOrEmpty(g.GrdWithdrawGrade)) && (!(string.IsNullOrEmpty(g.GrdGradeScheme)))))
                {
                    try
                    {
                        var gradeObject = grades.Where(g => g.Id == repoGrade.GrdWithdrawGrade && g.GradeSchemeCode == repoGrade.GrdGradeScheme).First();
                        gradeObject.IsWithdraw = true;
                    }
                    catch
                    {
                        // Log the original exception and a serialized version of the course record
                        logger.Error("Withdraw Grade ID " + repoGrade.GrdWithdrawGrade + " Scheme ID " + repoGrade.GrdGradeScheme + " does not exist in list of Grades built from repository.");
                    }
                }
                // Loop through the phone reg drop grades and update the boolean in each respective grade object
                List<RegDefaultsRgdPhoneDrops> phoneRegDropGrades = await PhoneRegDropGradesAsync();
                if (phoneRegDropGrades != null)
                {
                    foreach (var dropGrade in phoneRegDropGrades.Where(g => !(string.IsNullOrEmpty(g.RgdPhoneDropGradeAssocMember)) && !(string.IsNullOrEmpty(g.RgdPhoneDropGradeSchemeAssocMember))))
                    {
                        try
                        {
                            var gradeObject = grades.Where(g => g.Id == dropGrade.RgdPhoneDropGradeAssocMember && g.GradeSchemeCode == dropGrade.RgdPhoneDropGradeSchemeAssocMember).First();
                            gradeObject.IsWithdraw = true;
                        }
                        catch
                        {
                            // Log the original exception and a serialized version of the course record
                            logger.Error("Drop Grade ID " + dropGrade.RgdPhoneDropGradeAssocMember + " Scheme ID " + dropGrade.RgdPhoneDropGradeSchemeAssocMember + " does not exist in list of Grades built from repository.");
                        }
                    }
                }
            }
            return grades;
        }

        /// <summary>
        /// Gets HeDM Grades
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag</param>
        /// <returns>Returns Grades collection</returns>
        public async Task<ICollection<Grade>> GetHedmAsync(bool bypassCache = false)
        {
            if (bypassCache)
            {
                Collection<Grades> gradesData = await DataReader.BulkReadRecordAsync<Grades>("");
                VerifyGrades(gradesData);
                // No need for a separate comparisonGradeData list, since gradesData contains all grade records
                var gradeList = await BuildGradesAsync(gradesData, null);
                return gradeList;
            }
            else
            {
                return await GetOrAddToCacheAsync<List<Grade>>("AllGrades",
                                async () =>
                                {
                                    Collection<Grades> gradesData = await DataReader.BulkReadRecordAsync<Grades>("");
                                    VerifyGrades(gradesData);
                                    // No need for a separate comparisonGradeData list, since gradesData contains all grade records
                                    var gradeList = await BuildGradesAsync(gradesData, null);
                                    return gradeList;
                                }
                            );
            }
        }

        /// <summary>
        /// Get guid for Grades code
        /// </summary>
        /// <param name="code">Grades code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetGradesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetHedmAsync(false);
            Grade codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Id.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetHedmAsync(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'GRADES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Id.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'GRADES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'GRADES', Record ID:'", code, "'"));
            }
            return guid;

        }


        /// <summary>
        /// Gets HeDM Grade based on Id
        /// </summary>
        /// <param name="id">Grade Id</param>
        /// <returns>Returns Grade</returns>
        public async Task<Grade> GetHedmGradeByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "id must be specified.");
            }

            var gradeId = await GetRecordKeyFromGuidAsync(id);

            if (string.IsNullOrEmpty(gradeId))
            {
                throw new KeyNotFoundException("Grade not found for id " + id);
            }
            try
            {
                return await GetNewGradeAsync(gradeId);
            }

            catch (ApplicationException aex)
            {
                throw aex;
            }
            catch (Exception)
            {
                throw new KeyNotFoundException("Grade not found for id " + id);
            }
        }

        /// <summary>
        /// Gets Entity Grade based on gradeId
        /// </summary>
        /// <param name="gradeId">Grade Id</param>
        /// <returns>Returns Entity Grade</returns>
        private async Task<Grade> GetNewGradeAsync(string gradeId)
        {
            if (string.IsNullOrEmpty(gradeId))
            {
                throw new ArgumentNullException("gradeId");
            }

            Grades grade = await DataReader.ReadRecordAsync<Grades>(gradeId);
            if (grade == null)
            {
                throw new KeyNotFoundException("Invalid grade ID: " + gradeId);
            }

            // If the grade has a comparison grade, reads its record also to pass to BuildGradesAsync
            Collection<Grades> comparisonGrades = null;
            if (!string.IsNullOrEmpty(grade.GrdComparisonGrade))
            {
                Grades comparisonGrade = await DataReader.ReadRecordAsync<Grades>(grade.GrdComparisonGrade);
                comparisonGrades = new Collection<Grades>() { comparisonGrade };
            }

            var grades = new Collection<Grades>() { grade };
            VerifyGrades(grades);

            // Note: BuildGradesAsync does not populate IsWithdrawn correctly from this call, because we don't pass the full list of grade objects
            // that it expects. Not fixing at this time because it is not in the scope of the current changes, and the callers of GetHedmGradeByIdAsync
            // do not use that value.
            // A couple of options for fixing in the future are:
            // 1. Read the necessary reference grades here, and pass to BuildGradesAsync. Consider performance effect of the additional read.
            // 2. Make a new GradeBase object that does not contain IsWithdraw, and return that object to the HEDM calls. Other existing calls
            //    would return Grade, which would be derived from GradeBase.

            var newGrade = await BuildGradesAsync(grades, comparisonGrades);

            return newGrade.FirstOrDefault();
        }

        private async Task<List<RegDefaultsRgdPhoneDrops>> PhoneRegDropGradesAsync()
        {
            var registrationDefaults = await GetOrAddToCacheAsync<Ellucian.Colleague.Data.Student.DataContracts.RegDefaults>("RegistrationDefaults",
              async () =>
              {
                  Data.Student.DataContracts.RegDefaults regDefaults = await DataReader.ReadRecordAsync<Data.Student.DataContracts.RegDefaults>("ST.PARMS", "REG.DEFAULTS");
                  if (regDefaults == null)
                  {
                      var errorMessage = "Unable to access REG.DEFAULTS to determine phone registration withdraw grade. Skipping logic.";
                      logger.Info(errorMessage);
                      regDefaults = new RegDefaults();
                  }
                  return regDefaults;
              }, Level1CacheTimeoutValue);
            return registrationDefaults.RgdPhoneDropsEntityAssociation;
        }


        private void VerifyGrades(Collection<Grades> gradesData)
        {

            // HEDM will not return grades missing grade schemes

            IEnumerable<Grades> badGrades = new List<Grades>();
            badGrades = gradesData.Where(gd => gd.RecordGuid != null && string.IsNullOrEmpty(gd.GrdGradeScheme));
            foreach (var badGrade in badGrades)
            {
                logger.Error("Grade with GUID " + badGrade.RecordGuid + " and ID " + badGrade.Recordkey + " is missing a Grade Scheme.");
            }

            if (badGrades.Count() == 1)
            {
                var msg = "Grade with GUID " + badGrades.First().RecordGuid + " and ID " + badGrades.First().Recordkey + " is missing a Grade Scheme.";
                throw new ApplicationException(msg);
            }

            if (badGrades.Count() > 0)
            {
                throw new ApplicationException("There are grades thaat have no associated grade schemes.  Please check the log for details.");
            }

        }



    }
}
