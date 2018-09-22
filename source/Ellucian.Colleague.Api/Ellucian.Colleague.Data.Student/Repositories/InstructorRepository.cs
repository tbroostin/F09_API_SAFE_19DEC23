//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class InstructorRepository : BaseColleagueRepository, IPersoRepository
    {
        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        /// 
        private readonly int bulkReadSize;
        public InstructorRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Gets instructors
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="instructor"></param>
        /// <param name="primaryLocation"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Domain.Student.Entities.Instructor>, int>> GetInstructorsAsync(int offset, int limit, string instructor, string primaryLocation, bool bypassCache)
        {
            var instructors = new List<Domain.Student.Entities.Instructor>();
            string criteria = string.Empty;
            var totalCount = 0;

            if (!string.IsNullOrEmpty(instructor))
            {
                string recordKey = await GetRecordKeyFromGuidAsync(instructor);
                if (string.IsNullOrEmpty(recordKey)) 
                {
                    return new Tuple<IEnumerable<Domain.Student.Entities.Instructor>, int>(instructors, 0);
                    //throw new KeyNotFoundException(string.Concat("Instructor key not found for guid: ", instructor));
                }
                criteria = string.Format("WITH FACULTY.ID EQ '{0}'", recordKey);
            }

            if (!string.IsNullOrEmpty(primaryLocation))
            {
                if (!string.IsNullOrEmpty(criteria))
                {
                    criteria += string.Format(" AND FAC.HOME.LOCATION EQ '{0}'", primaryLocation);
                }
                else
                {
                    criteria = string.Format("FAC.HOME.LOCATION EQ '{0}'", primaryLocation);
                }
            }

            var instructorsIds = await DataReader.SelectAsync("FACULTY", criteria);
            totalCount = instructorsIds.Count();

            if (totalCount == 0)
            {
                return new Tuple<IEnumerable<Domain.Student.Entities.Instructor>, int>(instructors, 0);
            }

            Array.Sort(instructorsIds);
            var sublist = instructorsIds.Skip(offset).Take(limit).ToArray();

            var facultyDataContract =
                await DataReader.BulkReadRecordAsync<DataContracts.Faculty>("FACULTY", sublist);

            //get active perstat data contracts for those faculty

            var perstatActRecords = await GetActivePerstatRecords(sublist);

            foreach (var faculty in facultyDataContract)
            {
                var perstat = new Perstat();
                if (perstatActRecords != null && perstatActRecords.Any())
                {
                    perstat = perstatActRecords.FirstOrDefault(per => per.PerstatHrpId == faculty.Recordkey);
                }
                instructors.Add(BuildFaculty(faculty, perstat));
            }

            return instructors.Any()?
                new Tuple<IEnumerable<Domain.Student.Entities.Instructor>, int>(instructors, totalCount) :
                new Tuple<IEnumerable<Domain.Student.Entities.Instructor>, int>(instructors, 0);

        }

        /// <summary>
        /// Get a collection of TenureTypes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of TenureTypes</returns>
        public async Task<IEnumerable<Domain.Student.Entities.TenureTypes>> GetTenureTypesAsync(bool ignoreCache)
        {
            //return empty entity if there is a problem with the data read if the institution does not have 
            try
            {
                return await GetGuidValcodeAsync<Domain.Student.Entities.TenureTypes>("HR", "TENURE.TYPES",
                    (cl, g) => new Domain.Student.Entities.TenureTypes(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                        ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
            }
            catch
            {
                var emptyTenureType = new List<Domain.Student.Entities.TenureTypes>();
                return emptyTenureType;
            }
        }

        /// <summary>
        /// Gets instructor
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<Domain.Student.Entities.Instructor> GetInstructorByIdAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Instructor guid is a required field.");
            }

            var recordInfo = await GetRecordInfoFromGuidAsync(guid);
            if (recordInfo == null || string.IsNullOrEmpty(recordInfo.PrimaryKey) || recordInfo.Entity != "FACULTY")
            {
                throw new KeyNotFoundException(string.Format("FACULTY record {0} does not exist.", guid));
            }

            var instructorId = recordInfo.PrimaryKey;
            if (string.IsNullOrEmpty(instructorId))
            {
                throw new KeyNotFoundException("Instructor key not found for GUID " + guid);
            }

            var instructorDataContract = await DataReader.ReadRecordAsync<Faculty>("FACULTY", instructorId);
            if (instructorDataContract == null)
            {
                throw new KeyNotFoundException("Instructor key not found for GUID " + guid);
            }
            //get active perstat for that ID
            var perstat = new Perstat();
            var perstatRecords = await GetActivePerstatRecords(new string[] { instructorId });
            if (perstatRecords != null && perstatRecords.Any())
                perstat = perstatRecords.FirstOrDefault();
            var instructorEntity = BuildFaculty(instructorDataContract, perstat);
            return instructorEntity;
        }

        /// <summary>
        /// Builds instructor entity
        /// </summary>
        /// <param name="faculty"></param>
        /// <returns></returns>
        private Domain.Student.Entities.Instructor BuildFaculty(Faculty faculty, Perstat perstat)
        {
            Domain.Student.Entities.Instructor instructor = new Domain.Student.Entities.Instructor(faculty.RecordGuid, faculty.Recordkey);

            instructor.Departments = BuildDepartments(faculty.DeptLoadEntityAssociation);
            instructor.HomeLocation = faculty.FacHomeLocation;
            instructor.SpecialStatus = faculty.FacSpecialStatus;
            instructor.ContractType = faculty.FacContractType;
            if (perstat != null)
            {
                instructor.TentureType = perstat.PerstatTenureType;
                instructor.TenureTypeDate = perstat.PerstatTenureTypeDate;
            }
            return instructor;
        }

        /// <summary>
        /// Builds departrments
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private IEnumerable<Domain.Student.Entities.FacultyDeptLoad> BuildDepartments(IEnumerable<FacultyDeptLoad> source)
        {
            List<Domain.Student.Entities.FacultyDeptLoad> depts = new List<Domain.Student.Entities.FacultyDeptLoad>();

            if (source == null)
            {
                return null;
            }

            foreach (var item in source)
            {
                Domain.Student.Entities.FacultyDeptLoad load = new Domain.Student.Entities.FacultyDeptLoad();
                load.DeptPcts = item.FacDeptPctsAssocMember;
                load.FacultyDepartment = item.FacDeptsAssocMember;
                depts.Add(load);
            }
            return depts;
        }

        /// <summary>
        /// Gets all the guids for the person keys
        /// </summary>
        /// <param name="personRecordKeys"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetPersonGuidsAsync(IEnumerable<string> personRecordKeys)
        {
            if (personRecordKeys != null && !personRecordKeys.Any())
            {
                return null;
            }

            var assessmentGuids = new Dictionary<string, string>();

            if (personRecordKeys != null && personRecordKeys.Any())
            {
                // convert the person keys to person guids
                var personGuidLookup = personRecordKeys.ToList().ConvertAll(p => new RecordKeyLookup("PERSON", p, false)).ToArray();
                var recordKeyLookupResults = await DataReader.SelectAsync(personGuidLookup);
                foreach (var recordKeyLookupResult in recordKeyLookupResults)
                {
                    string[] splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!assessmentGuids.ContainsKey(splitKeys[1]))
                    {
                        if (recordKeyLookupResult.Value != null)
                        {
                            assessmentGuids.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                        }
                    }
                }
            }
            return (assessmentGuids != null && assessmentGuids.Any()) ? assessmentGuids : null;
        }

        private async Task<List<Perstat>> GetActivePerstatRecords(string[] personIds)
        {
            //put this in try catch and ignore the error to deal with issue where client might not have the license for the HR module 
            //get all the related perstart records for those faculty if they have one. 
            var perstatActRecords = new List<Perstat>();
            try
            {
                // select all the PERSTAT ids with the HRP.ID equal to the input person id.
                string criteria = "WITH PERSTAT.HRP.ID EQ ?";
                var perstatKeys = await DataReader.SelectAsync("PERSTAT", criteria, personIds.Select(id => string.Format("\"{0}\"", id)).ToArray());
                var perstatAllRecords = new List<Perstat>();
                for (int i = 0; i < perstatKeys.Count(); i += bulkReadSize)
                {
                    var subList = perstatKeys.Skip(i).Take(bulkReadSize);
                    var records = await DataReader.BulkReadRecordAsync<Perstat>(subList.ToArray());
                    if (records != null)
                    {
                        perstatAllRecords.AddRange(records);
                    }
                }
                //out of those perstat record, we just want the active ones. 
                if (perstatAllRecords != null && perstatAllRecords.Any())
                {
                    foreach (var perstat in perstatAllRecords)
                    {
                        if (perstat.PerstatEndDate == null || perstat.PerstatEndDate > DateTime.Today)
                        {
                            if (perstat.PerstatStartDate <= DateTime.Today)
                            {
                                perstatActRecords.Add(perstat);
                            }
                        }

                    }
                }

            }
            catch
            { }
            return perstatActRecords;
        }
    }
}
