// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
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
    public class StudentStandingRepository : BaseColleagueRepository, IStudentStandingRepository
    {
        public StudentStandingRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Get Student Standings for multiple students
        /// </summary>
        /// <param name="studentIds">List of student Ids to get Student Standings for</param>
        /// <param name="term">Term to filter student standings</param>
        /// <returns>Returns a list of Student Standing Entities</returns>
        public async Task<IEnumerable<StudentStanding>> GetAsync(IEnumerable<string> studentIds, string term = null)
        {
            List<StudentStanding> studentStandings = new List<StudentStanding>();
            bool error = false;

            if (studentIds != null && studentIds.Count() > 0)
            {
                // get students data
                string[] studentStandingIds = await DataReader.SelectAsync("STUDENT.STANDINGS", " WITH STS.STUDENT = '?'", studentIds.ToArray());
                Collection<StudentStandings> studentStandingData = await DataReader.BulkReadRecordAsync<StudentStandings>("STUDENT.STANDINGS", studentStandingIds);
                if (studentStandingData == null)
                    logger.Debug(string.Format("No student standing data returned for students {0}", string.Join(",", studentIds)));
                foreach (var studentStanding in studentStandingData)
                {
                    if (studentStanding != null)
                    {
                        try
                        {
                            var studentStandingEntity = BuildStandings(studentStanding, term);
                            if (studentStandingEntity != null)
                            {
                                studentStandings.Add(studentStandingEntity);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(string.Format("Failed to build student standing {0} for student {1}", studentStanding.Recordkey, studentStanding.StsStudent));
                            logger.Error(e.GetBaseException().Message);
                            logger.Error(e.GetBaseException().StackTrace);
                            error = true;
                        }
                    }
                }
            }
            if (error && studentStandings.Count() == 0)
                throw new Exception("Unexpected errors occurred. No student standings records returned. Check API error log.");

            return studentStandings;
        }

        /// <summary>
        /// Build the Student Standings Entity from the Data Contract
        /// </summary>
        /// <param name="studentStanding">Data Contract object for Student Standings</param>
        /// <param name="term">Term to filter student standings</param>
        /// <returns>A single Entity object for Student Standings</returns>
        private StudentStanding BuildStandings(StudentStandings studentStanding, string term = null)
        {
            // For now we are only returning book price information, but will eventually need book details.
            StudentStanding studentStandingEntity = null;
            if (studentStanding != null)
            {
                try
                {
                    studentStandingEntity = new StudentStanding(studentStanding.Recordkey, studentStanding.StsStudent, studentStanding.StsAcadStanding, studentStanding.StsAcadStandingDate.GetValueOrDefault());
                    studentStandingEntity.Level = studentStanding.StsAcadLevel;
                    studentStandingEntity.Program = studentStanding.StsAcadProgram;
                    studentStandingEntity.Term = studentStanding.StsTerm;
                    studentStandingEntity.CalcStandingCode = studentStanding.StsCalcAcadStanding;
                    studentStandingEntity.OverrideReason = studentStanding.StsOverrideReason;

                    switch (studentStanding.StsType)
                    {
                        case "ACLV":
                        {
                            studentStandingEntity.Type = StudentStandingType.AcademicLevel;
                            break;
                        }
                        case "ACPG":
                        {
                            studentStandingEntity.Type = StudentStandingType.Program;
                            break;
                        }
                        case "TERM":
                        {
                            studentStandingEntity.Type = StudentStandingType.Term;
                            break;
                        }
                        default:
                        {
                            throw new NotSupportedException("Unexpected condition encountered in Student Standing of Type " + studentStanding.StsType);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogDataError("Student Standings", studentStanding.Recordkey, studentStanding, ex);
                    //throw;
                    return null;
                }
                if (term != null)
                {
                    if (term == studentStandingEntity.Term)
                    {
                        return studentStandingEntity;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return studentStandingEntity;
                }
            }
            return studentStandingEntity;
        }

        public async Task<Dictionary<string, List<StudentStanding>>> GetGroupedAsync(IEnumerable<string> studentIds)
        {
            Dictionary<string, List<StudentStanding>> standingsByStudent = new Dictionary<string, List<StudentStanding>>();

            bool error = false;

            if (studentIds != null && studentIds.Count() > 0)
            {
                // get students data
                string[] studentStandingIds = await DataReader.SelectAsync("STUDENT.STANDINGS", " WITH STS.STUDENT = '?'", studentIds.ToArray());
                Collection<StudentStandings> studentStandingData = await DataReader.BulkReadRecordAsync<StudentStandings>("STUDENT.STANDINGS", studentStandingIds);
                if (studentStandingData == null)
                    logger.Debug(string.Format("No student standing data returned for students {0}", string.Join(",", studentIds)));

                foreach (var studentStanding in studentStandingData)
                {
                    if (studentStanding != null)
                    {
                        try
                        {
                            var studentStandingEntity = BuildStandings(studentStanding);
                            if (studentStandingEntity != null)
                            {
                                if (!standingsByStudent.ContainsKey(studentStandingEntity.StudentId))
                                    standingsByStudent[studentStandingEntity.StudentId] = new List<StudentStanding>();
                                standingsByStudent[studentStandingEntity.StudentId].Add(studentStandingEntity);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(string.Format("Failed to build student standing {0} for student {1}", studentStanding.Recordkey, studentStanding.StsStudent));
                            logger.Error(e.GetBaseException().Message + e.GetBaseException().StackTrace);
                            error = true;
                        }
                    }
                }
            }
            if (error && standingsByStudent.Count() == 0)
                throw new Exception("Errors prevented partial return of student standings batch.");

            return standingsByStudent;
        }

        /// <summary>
        ///  Get collection of StudentStandings
        /// </summary>
        /// <param name="offset">Paging offset</param>
        /// <param name="limit">Paging limit</param>
        /// <returns>collection of StudentStandings</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentStanding>, int>> GetStudentStandingsAsync(int offset, int limit)
        {
            var studentStandingIds = await DataReader.SelectAsync("STUDENT.STANDINGS", "WITH STS.STUDENT NE ''");
            var totalCount = studentStandingIds.Count();
            Array.Sort(studentStandingIds);
            var subList = studentStandingIds.Skip(offset).Take(limit).ToArray();

            var studentStandingData = await DataReader.BulkReadRecordAsync<DataContracts.StudentStandings>("STUDENT.STANDINGS", subList);

            if (studentStandingData == null)
            {
                throw new KeyNotFoundException("No records selected from StudentStandings file in Colleague.");
            }

            var studentStandings = BuildStudentStandings(studentStandingData);

            return new Tuple<IEnumerable<StudentStanding>, int>(studentStandings, totalCount);

        }

        /// <summary>
        /// Get StudentStanding domain entity By Guid
        /// </summary>
        /// <param name="guid">Unique identifier</param>
        /// <returns>StudentStanding domain entity</returns>
        public async Task<StudentStanding> GetStudentStandingByGuidAsync(string guid)
        {
            try
            {
                var id = await GetStudentStandingIdFromGuidAsync(guid);
                if (string.IsNullOrEmpty(id))
                {
                    throw new KeyNotFoundException(string.Concat("Id not found for StudentStanding guid:", guid));
                }
                return await GetStudentStandingAsync(id);
            }
            catch (Exception e)
            {
                throw new KeyNotFoundException("StudentStanding GUID " + guid + " lookup failed.");
            }
        }

        /// <summary>
        /// Get a single StudentStanding domain entity using an ID
        /// </summary>
        /// <param name="id">The StudentStanding ID</param>
        /// <returns>The StudentStanding domain entity</returns>
        private async Task<StudentStanding> GetStudentStandingAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a StudentStanding.");
            }

            // Now we have an ID, so we can read the record
            var studentStandings = await DataReader.ReadRecordAsync<StudentStandings>(id);
            if (studentStandings == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found for StudentStanding with ID ", id, "invalid."));
            }

            var result = BuildStandings(studentStandings);
            result.Guid = studentStandings.RecordGuid;
            return result;
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        private async Task<string> GetStudentStandingIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("StudentStanding GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("StudentStanding GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "STUDENT.STANDINGS")
            {
                throw new RepositoryException("GUID " + guid + " has different entity, " + foundEntry.Value.Entity + ", than expected, VENDORS");
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        ///  Build collection of StudentStanding domain entities from collection of StudentStandings data contacts
        /// </summary>
        /// <param name="studentStandings">Collection of StudentStandings data contracts</param>
        /// <returns>Collection of StudentStanding domain entity</returns>
        private IEnumerable<StudentStanding> BuildStudentStandings(IEnumerable<StudentStandings> studentStandings)
        {
            var studentStandingsCollection = new List<StudentStanding>();

            foreach (var studentStanding in studentStandings)
            {
                var result = BuildStandings(studentStanding);
                result.Guid = studentStanding.RecordGuid;
                studentStandingsCollection.Add(result);
            }

            return studentStandingsCollection.AsEnumerable();
        }
    }
}