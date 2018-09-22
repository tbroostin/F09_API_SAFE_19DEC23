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

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentAdvisorRelationshipsRepository : BaseColleagueRepository, IStudentAdvisorRelationshipsRepository
    {

        public StudentAdvisorRelationshipsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
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
            Tuple<IEnumerable<StudentAdvisorRelationship>, int> emptySet = new Tuple<IEnumerable<StudentAdvisorRelationship>, int>(new List<StudentAdvisorRelationship>(), 0);

            bool haveStudent = !string.IsNullOrEmpty(student);
            bool haveAdvisor = !string.IsNullOrEmpty(advisor);

            if (haveStudent || haveAdvisor)
            {

                List<string> studentAdvisementKeyList = new List<string>();
                List<string> advisorAdvisementKeyList = new List<string>();
                List<string> finalAdvisementKeyList = new List<string>();

                // If student provided, get PST.ADVISEMENT list from PERSON.ST record
                if (haveStudent)
                {
                    var studentPersonSt = await DataReader.ReadRecordAsync<PersonSt>(student);
                    if (studentPersonSt == null || studentPersonSt.PstAdvisement == null || studentPersonSt.PstAdvisement.Count == 0) return emptySet;
                    studentAdvisementKeyList.AddRange(studentPersonSt.PstAdvisement);
                }

                // If advisor provided, get FAC.ADVISEES list from FACULTY record
                if (haveAdvisor)
                {
                    var advisorFac= await DataReader.ReadRecordAsync<DataContracts.Faculty>(advisor);
                    if (advisorFac == null || advisorFac.FacAdvisees == null || advisorFac.FacAdvisees.Count == 0) return emptySet;
                    advisorAdvisementKeyList.AddRange(advisorFac.FacAdvisees);
                }

                // If we have both, then an intersection of the lists will be the advisement records they have in common.
                if (haveAdvisor && haveStudent)
                {
                    finalAdvisementKeyList = studentAdvisementKeyList.Intersect(advisorAdvisementKeyList).ToList();
                    if (finalAdvisementKeyList.Count == 0) return emptySet;
                }
                else if (haveAdvisor)
                {
                    finalAdvisementKeyList = advisorAdvisementKeyList;
                }
                else // (haveStudent)
                {
                    finalAdvisementKeyList = studentAdvisementKeyList;
                }

                // Read the data
                studentAdvisementsData = await DataReader.BulkReadRecordAsync<StudentAdvisement>(finalAdvisementKeyList.ToArray(), true);

                if (studentAdvisementsData == null)
                {
                    throw new KeyNotFoundException("No records selected from STUDENT.ADVISEMENT file in Colleague.");
                }
                // Build the entities
                foreach (var studentAdvisement in studentAdvisementsData)
                {
                    studentAdvisorRelationshipsEntities.Add(BuildStudentAdvisorRelationship(studentAdvisement));
                }

            }
            else
            {
                // No student or advisor - this is either an unfettered GET ALL or only limited by type; read the data and cache

                studentAdvisorRelationshipsEntities = await GetOrAddToCacheAsync<List<StudentAdvisorRelationship>>("AllStudentAdvisorRelationships",
                async () =>
                {
                    // Get advisement data from the database if not in cache. 
                    studentAdvisementsData = await DataReader.BulkReadRecordAsync<StudentAdvisement>("STUDENT.ADVISEMENT", "", true);
                    if (studentAdvisementsData == null)
                    {
                        throw new KeyNotFoundException("No records selected from STUDENT.ADVISEMENT file in Colleague.");
                    }
                    // Build the entities
                    List<StudentAdvisorRelationship> stadEntities = new List<StudentAdvisorRelationship>();
                    foreach (var studentAdvisement in studentAdvisementsData)
                    {
                        stadEntities.Add(BuildStudentAdvisorRelationship(studentAdvisement));
                    }
                    return stadEntities;
                });
            }

            // At this point, whether filtered by advisor/student or not, we have a list of entities.  
            // Filter on advisorType if needed; apply offset and limit; and return

            if (!string.IsNullOrEmpty(advisorType))
            {
                studentAdvisorRelationshipsEntities.RemoveAll(stad => stad.advisorType != advisorType);
            }

            if (studentAdvisorRelationshipsEntities.Count == 0) return emptySet;
            return new Tuple<IEnumerable<StudentAdvisorRelationship>, int>(studentAdvisorRelationshipsEntities.Skip(offset).Take(limit),
                                                                           studentAdvisorRelationshipsEntities.Count);
            
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
            StudentAdvisorRelationship sar = new StudentAdvisorRelationship();
            sar.id = studentAdvisement.Recordkey;
            sar.guid = studentAdvisement.RecordGuid;
            sar.advisor = studentAdvisement.StadFaculty;
            sar.student = studentAdvisement.StadStudent;
            sar.advisorType = studentAdvisement.StadType;
            sar.program = studentAdvisement.StadAcadProgram;
            sar.startOn = studentAdvisement.StadStartDate;
            sar.endOn = studentAdvisement.StadEndDate;

            return sar;
        }
    }
}
