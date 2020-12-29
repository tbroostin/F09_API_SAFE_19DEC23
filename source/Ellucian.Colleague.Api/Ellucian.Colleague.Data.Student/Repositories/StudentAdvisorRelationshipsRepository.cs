//Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Colleague.Data.Student.DataContracts;
using slf4net;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Services;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentAdvisorRelationshipsRepository : BaseColleagueRepository, IStudentAdvisorRelationshipsRepository
    {
        private RepositoryException exception;
        const string AllStudentAdvisorsCache = "AllStudentAdvisorsCache";
        const int AllStudentAdvisorsCacheTimeout = 20;

        public StudentAdvisorRelationshipsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            exception = new RepositoryException();
        }
                
        /// <summary>
        /// Get all records for StudentAdvisorRelationShips from STUDENT.ADVISEMENT entity. This method employs filters and paging.
        /// </summary>
        /// <param name="offset">Used to offset the values</param>
        /// <param name="limit">Used to limit the number of records</param>
        /// <param name="bypassCache"></param>
        /// <param name="student">Filter the data by student ID</param>
        /// <param name="advisor">Filter the data by faculty ID</param>
        /// <param name="advisorType">Filter the records by advisor type</param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<StudentAdvisorRelationship>, int>> GetStudentAdvisorRelationshipsAsync(int offset, int limit, bool bypassCache = false,
            string student = "", string advisor = "", string advisorType = "")
        {
            IEnumerable<StudentAdvisement> studentAdvisementsData;
            List<StudentAdvisorRelationship> studentAdvisorRelationshipsEntities = new List<StudentAdvisorRelationship>();
            //Tuple<IEnumerable<StudentAdvisorRelationship>, int> emptySet = new Tuple<IEnumerable<StudentAdvisorRelationship>, int>(new List<StudentAdvisorRelationship>(), 0);
            var totalCount = 0;
            List<string> limitingKeys = new List<string>();
            var criteria = "WITH STAD.STUDENT NE '' AND WITH STAD.FACULTY NE '' AND STAD.START.DATE NE ''";
            try
            {
                string studentAdvisementCacheKey = CacheSupport.BuildCacheKey(AllStudentAdvisorsCache, student, advisor, advisorType);

                var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
                   this,
                   ContainsKey,
                   GetOrAddToCacheAsync,
                   AddOrUpdateCacheAsync,
                   transactionInvoker,
                   studentAdvisementCacheKey,
                   "STUDENT.ADVISEMENT",
                   offset,
                   limit,
                   AllStudentAdvisorsCacheTimeout,
                   async () =>
                   {
                       string[] keys = null;
                       List<string> stLimitingKeys = new List<string>();
                       List<string> advLimitingKeys = new List<string>();
                       if (!string.IsNullOrWhiteSpace(student))
                       {
                           var studentPersonSt = await DataReader.ReadRecordAsync<PersonSt>(student);
                           if (studentPersonSt == null || studentPersonSt.PstAdvisement == null || studentPersonSt.PstAdvisement.Count == 0)
                           {
                               return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                           }
                           stLimitingKeys.AddRange(studentPersonSt.PstAdvisement);
                       }

                       if (!string.IsNullOrWhiteSpace(advisor))
                       {
                           var advisorFac = await DataReader.ReadRecordAsync<DataContracts.Faculty>(advisor);
                           if ((advisorFac == null || advisorFac.FacAdvisees == null || !advisorFac.FacAdvisees.Any()) ||
                           (string.IsNullOrEmpty(advisorFac.FacAdviseFlag) || !advisorFac.FacAdviseFlag.Equals("Y", StringComparison.OrdinalIgnoreCase)))
                           {
                               return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                           }
                           advLimitingKeys.AddRange(advisorFac.FacAdvisees);
                       }
                       if (!string.IsNullOrWhiteSpace(advisorType))
                       {
                           criteria = string.Format("{0} AND STAD.TYPE EQ '{1}'", criteria, advisorType);
                       }

                       if(!string.Concat(student, advisor).Equals(string.Empty) && (stLimitingKeys != null && stLimitingKeys.Any()) && (advLimitingKeys != null && advLimitingKeys.Any()))
                       {
                           limitingKeys = stLimitingKeys.Intersect(advLimitingKeys).ToList();
                       }
                       else if(stLimitingKeys != null && stLimitingKeys.Any())
                       {
                           limitingKeys = stLimitingKeys;
                       }
                       else if(advLimitingKeys != null && advLimitingKeys.Any())
                       {
                           limitingKeys = advLimitingKeys;
                       }

                       if (limitingKeys != null && limitingKeys.Any())
                       {
                           limitingKeys = limitingKeys.Distinct().ToList();
                       }

                       keys = await DataReader.SelectAsync("STUDENT.ADVISEMENT", limitingKeys.ToArray(), criteria);

                       if(keys == null || !keys.Any())
                       {
                           return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                       }

                       return new CacheSupport.KeyCacheRequirements()
                       {
                           limitingKeys = keys != null && keys.Any() ? keys.Distinct().ToList() : null,
                           criteria = criteria,
                       };
                   });
                
                if (keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any())
                {
                    return new Tuple<IEnumerable<StudentAdvisorRelationship>, int>(new List<StudentAdvisorRelationship>(), 0);
                }

                totalCount = keyCacheObject.TotalCount.Value;
                var sublist = keyCacheObject.Sublist.ToArray();

                // Read the data
                studentAdvisementsData = await DataReader.BulkReadRecordAsync<StudentAdvisement>("STUDENT.ADVISEMENT", sublist);

                if (studentAdvisementsData == null)
                {
                    exception.AddError(new RepositoryError("Bad.Data", "No records selected from STUDENT.ADVISEMENT file in Colleague."));
                    throw exception;
                }
                // Build the entities
                foreach (var studentAdvisement in studentAdvisementsData)
                {
                    try
                    {
                        studentAdvisorRelationshipsEntities.Add(BuildStudentAdvisorRelationship(studentAdvisement));
                    }
                    catch (Exception e)
                    {
                        exception.AddError(new RepositoryError("Bad.Data", e.Message));
                    }
                }

                if (exception != null && exception.Errors != null && exception.Errors.Any())
                {
                    throw exception;
                }
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception e)
            {
                exception.AddError(new RepositoryError("Bad.Data", e.Message));
                throw exception;
            }

            return studentAdvisorRelationshipsEntities != null || studentAdvisorRelationshipsEntities.Any() ?
                new Tuple<IEnumerable<StudentAdvisorRelationship>, int>(studentAdvisorRelationshipsEntities, totalCount) :
                new Tuple<IEnumerable<StudentAdvisorRelationship>, int>(new List<StudentAdvisorRelationship>(), 0);

        }

        /// <summary>
        /// Get all records for StudentAdvisorRelationShips from STUDENT.ADVISEMENT entity. This method uses the filters and paging.
        /// </summary>
        /// <param name="offset">Used to offset the values</param>
        /// <param name="limit">Used to limit the number of records</param>
        /// <param name="bypassCache"></param>
        /// <param name="student">Filter the data by student ID</param>
        /// <param name="advisor">Filter the data by faculty ID</param>
        /// <param name="advisorType">Filter the records by advisor type</param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<StudentAdvisorRelationship>,int>> GetStudentAdvisorRelationshipsOldAsync(int offset, int limit, bool bypassCache = false,
            string student = "", string advisor = "", string advisorType = "")
        {
            StringBuilder criteria = new StringBuilder();
            //make sure we get records based on required fields.
            criteria.Append("WITH STAD.STUDENT NE '' AND STAD.FACULTY NE '' AND STAD.START.DATE NE ''");

            // filter by student ID
            if (!string.IsNullOrEmpty(student))
            {
                criteria.Append(" AND WITH STAD.STUDENT = '");
                criteria.Append(student);
                criteria.Append("'");
            }

            //filter by Advisor ID
            if (!string.IsNullOrEmpty(advisor))
            {
                criteria.Append(" AND WITH STAD.FACULTY = '");
                criteria.Append(advisor);
                criteria.Append("'");
            }

            //Fliter by Advisor type
            if (!string.IsNullOrEmpty(advisorType))
            {
                criteria.Append(" AND WITH STAD.TYPE = '");
                criteria.Append(advisorType);
                criteria.Append("'");
            }

            //Get all ID's based on criteria
            var studentAdvisementIds = await DataReader.SelectAsync("STUDENT.ADVISEMENT", criteria.ToString());
            
            // update total count and get the ID's we want to page off of
            var totalCount = studentAdvisementIds.Count();
            Array.Sort(studentAdvisementIds);
            var subList = studentAdvisementIds.Skip(offset).Take(limit).ToArray();

            //get all records data and parse through it to add to our entity
            var studentAdvisementsData = await DataReader.BulkReadRecordAsync<DataContracts.StudentAdvisement>("STUDENT.ADVISEMENT", subList);

            if (studentAdvisementsData == null)
            {
                throw new KeyNotFoundException("No records selected from STUDENT.ADVISEMENT file in Colleague.");
            }

            List<StudentAdvisorRelationship> StudentAdvisorRelationshipsEntity = new List<StudentAdvisorRelationship>();


            foreach (var studentAdvisement in studentAdvisementsData)
            {
                StudentAdvisorRelationshipsEntity.Add(BuildStudentAdvisorRelationship(studentAdvisement));
            }

            return new Tuple<IEnumerable<StudentAdvisorRelationship>, int>(StudentAdvisorRelationshipsEntity.AsEnumerable(), totalCount);

        }

        /// <summary>
        /// Get a student advisor Relationship based on guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<StudentAdvisorRelationship> GetStudentAdvisorRelationshipsByGuidAsync(string guid)
        {
           
            try
            {
                //tranlate the GUID to a valid ID
                string id = await GetStudentAdvisementIdFromGuidAsync(guid);
                if (string.IsNullOrEmpty(id))
                {
                    throw new KeyNotFoundException(string.Concat("Id not found for Student Advisement guid:", guid));
                }
                //Parse the ID to the Entity
                return await GetStudentAdvisementAsync(id);
            }
            catch (ArgumentNullException e)
            {
                logger.Error(e.Message);
                throw new ArgumentNullException(e.Message);
            }
            catch (ArgumentException e)
            {
                logger.Error(e.Message);
                throw new ArgumentException(e.Message);
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.Message);
                throw new KeyNotFoundException(e.Message);
            }
            catch (RepositoryException e)
            {
               var RepoExc = new RepositoryException(e.Message, e);
               RepoExc.AddError(new RepositoryError("student.advisement.GetByGUID",e.Message));
               throw RepoExc;
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw new KeyNotFoundException("STUDENT.ADVISEMENT GUID " + guid + " lookup failed.");
            }
            
        }

        /// <summary>
        /// Get the ID of STUDENT.ADVISOMENT from a GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private async Task<string> GetStudentAdvisementIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "STUDENT.ADVISEMENT - GUID can not be null or empty");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                //Guid was not found
                throw new KeyNotFoundException("STUDENT.ADVISEMENT GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                //Guid was not found
                throw new KeyNotFoundException("STUDENT.ADVISEMENT GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "STUDENT.ADVISEMENT")
            {
                //GUID record was not from a STUDENT.ADVISEMENT record
                throw new RepositoryException("GUID used '" + guid + "' is for a different entity in colleague, it is for" + foundEntry.Value.Entity + ". We need the entity to be STUDENT.ADVISEMENT");
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Get the STUDENT.ADVISEMENT record by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<StudentAdvisorRelationship> GetStudentAdvisementAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a STUDENT.ADVISEMENT.");
            }

            var studentAdvisement = await DataReader.ReadRecordAsync<StudentAdvisement>(id);
            if (studentAdvisement == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found for STUDENT.ADVISEMENT with ID ", id, "invalid."));
            }

            return BuildStudentAdvisorRelationship(studentAdvisement);
        }

        /// <summary>
        /// Get a STUDENT.ADVISEMENT record and convert it to a StudentAdvisorRelationship entity
        /// </summary>
        /// <param name="studentAdvisement"></param>
        /// <returns></returns>
        private StudentAdvisorRelationship BuildStudentAdvisorRelationship(StudentAdvisement studentAdvisement)
        {
            StudentAdvisorRelationship sar = new StudentAdvisorRelationship(studentAdvisement.Recordkey, studentAdvisement.RecordGuid, 
                studentAdvisement.StadFaculty, studentAdvisement.StadStudent, studentAdvisement.StadStartDate)
            {
                AdvisorType = studentAdvisement.StadType,
                Program = studentAdvisement.StadAcadProgram,
                EndOn = studentAdvisement.StadEndDate
            };
            return sar;
        }
    }
}
