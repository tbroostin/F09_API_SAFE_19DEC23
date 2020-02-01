// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Planning.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Planning.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class AdvisorRepository : PersonRepository, IAdvisorRepository
    {
        readonly int readSize;

        public AdvisorRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger, apiSettings)
        {
            // Not currently caching.
            //CacheTimeout = 5;
            readSize = apiSettings.BulkReadSize;
        }


        /// <summary>
        /// Get full advisor Entity for specified Advisor, including advisement information. Throws an exception if specified ID is
        /// not found to be either FACULTY or STAFF(must also be categorized as staff).
        /// </summary>
        /// <param name="id">Id of the Advisor to retrieve</param>
        /// <returns>Advisor entity</returns>
        public async Task<Advisor> GetAsync(string id, AdviseeInclusionType adviseeInclusionType = AdviseeInclusionType.AllAdvisees)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Must provide the advisor id.");
            }

            try
            {
                // Call the batch get to retrieve a single advisor id.
                // Since this is a single get, throw an exception of not returned.
                return (await GetAdvisorsAsync(new List<string>() { id }, adviseeInclusionType)).First();
            }
            catch (Exception)
            {
                throw new KeyNotFoundException("Cannot find an advisor with the id " + id);
            }
        }

        /// <summary>
        /// Returns basic entity for the given advisor IDs (name, email address).
        /// Must be found on either faculty or registered as a staff member to be included in their response.
        /// </summary>
        /// <param name="advisorIds">IDs of the advisors to retrieve</param>
        /// <param name="onlyActiveAdvisees">If true, only return the IDs of currently active advisees</param>
        /// <param name="includeAdvisees">If true, include advisee data for the advisor</param>
        /// <returns>List of Advisor entities (not fully built out)</returns>
        public async Task<IEnumerable<Advisor>> GetAdvisorsAsync(IEnumerable<string> advisorIds, AdviseeInclusionType adviseeInclusionType = AdviseeInclusionType.AllAdvisees)
        {
            if (advisorIds == null || advisorIds.Count() == 0)
            {
                throw new ArgumentNullException("advisorIds", "Must provide the advisor ids.");
            }

            var advisors = new List<Advisor>();

            try
            {

                // Remove any duplicates from the list
                advisorIds = advisorIds.Distinct().ToList();

                // Bulk read the advisor PERSON records in chunks from the database.
                var personData = new List<Ellucian.Colleague.Data.Base.DataContracts.Person>();
                for (int i = 0; i < advisorIds.Count(); i += readSize)
                {
                    var subList = advisorIds.Skip(i).Take(readSize).ToArray();
                    var bulkData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(subList);
                    if (bulkData != null)
                    {
                        personData.AddRange(bulkData);
                    }
                    else
                    {
                        logger.Error("Failed to read PERSON database records for id list " + subList.ToString());
                    }
                }

                // If no person data retrieved, return an empty advisors list rather than continuing to attempt further database reads.
                if (personData == null || personData.Count() == 0)
                {
                    logger.Error("Unable to get PERSON information for specified advisor(s)");
                    return advisors;
                }

                // Bulk read the advisor FACULTY records in chunks from the database.
                var facultyData = new List<Ellucian.Colleague.Data.Student.DataContracts.Faculty>();
                for (int i = 0; i < advisorIds.Count(); i += readSize)
                {
                    var subList = advisorIds.Skip(i).Take(readSize).ToArray();
                    var bulkData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Faculty>("FACULTY", subList);
                    if (bulkData != null)
                    {
                        facultyData.AddRange(bulkData);
                    }
                    else
                    {
                        logger.Error("Failed to read FACULTY database records for id list " + subList.ToString());
                    }

                }

                var studentAdvisementData = new List<Data.Student.DataContracts.StudentAdvisement>();

                // Do not bother to read STUDENT.ADVISEMENT data if not returning advisees.
                if (adviseeInclusionType != AdviseeInclusionType.NoAdvisees)
                {

                    List<string> studentAdvisementIds = facultyData.Where(f => f.FacAdvisees != null && f.FacAdvisees.Count() > 0).SelectMany(f => f.FacAdvisees).Distinct().ToList();
                                       
                    if (studentAdvisementIds != null && studentAdvisementIds.Any())
                    {
                        // bulk read the selected advisements in chunks from the database. 

                        for (int i = 0; i < studentAdvisementIds.Count(); i += readSize)
                        {
                            var subList = studentAdvisementIds.Skip(i).Take(readSize).ToArray();
                            var bulkData = await DataReader.BulkReadRecordAsync<StudentAdvisement>("STUDENT.ADVISEMENT", subList.ToArray());
                            if (bulkData != null)
                            {
                                studentAdvisementData.AddRange(bulkData);
                            }
                            else
                            {
                                logger.Error("Failed to read STUDENT.ADVISEMENT database records for id list " + subList.ToString());
                            }
                        }
                    }
                }

                // Bulk read the advisor STAFF records in chunks from the database.
                // Read STAFF only for faculty without the advisement flag set to Yes.
                var advisingFacultyIds = facultyData.Where(f => f.FacAdviseFlag != null && f.FacAdviseFlag.ToUpper() == "Y").Select(f => f.Recordkey).Distinct();
                var staffIds = advisorIds.Except(advisingFacultyIds).Distinct();
                var staffData = new List<Ellucian.Colleague.Data.Base.DataContracts.Staff>();
                if (staffIds.Count() > 0)
                {
                    for (int i = 0; i < staffIds.Count(); i += readSize)
                    {
                        var subList = staffIds.Skip(i).Take(readSize).ToArray();
                        var bulkData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Staff>("STAFF", subList);
                        if (bulkData != null)
                        {
                            staffData.AddRange(bulkData);
                        }
                        else
                        {
                            logger.Error("Failed to read STAFF database records for id list " + subList.ToString());
                        }
                    }
                }

                logger.Info("Data retrieval for advisors is complete. Commencing build of advisors");

                // Build list of advisors using collected data
                advisors = await BuildAdvisorsAsync(advisorIds, personData, facultyData, studentAdvisementData, staffData, adviseeInclusionType);
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while trying to retrieve data to build advisors: " + ex.Message);
            }
            return advisors;
        }

        // With all the needed colleague data, build a list of advisor entities for the given advisor Ids
        private async Task<List<Advisor>> BuildAdvisorsAsync(IEnumerable<string> advisorIds,
                                            IEnumerable<Data.Base.DataContracts.Person> personData,
                                            IEnumerable<Data.Student.DataContracts.Faculty> facultyData,
                                            IEnumerable<Data.Student.DataContracts.StudentAdvisement> studentAdvisementData,
                                            IEnumerable<Data.Base.DataContracts.Staff> staffData,
                                            AdviseeInclusionType adviseeInclusionType)
        {
            var advisors = new List<Advisor>();

            try
            {
                // Put colleague data into Id-based dictionaries.
                var personDict = personData != null ? personData.ToDictionary(p => p.Recordkey, p => p) : new Dictionary<string, Data.Base.DataContracts.Person>();
                var facultyDict = facultyData != null ? facultyData.ToDictionary(f => f.Recordkey, f => f) : new Dictionary<string, Data.Student.DataContracts.Faculty>();
                var staffDict = staffData != null ? staffData.ToDictionary(s => s.Recordkey, s => s) : new Dictionary<string, Data.Base.DataContracts.Staff>();
                var studentAdvisementDict = studentAdvisementData != null ? studentAdvisementData.ToDictionary(sa => sa.Recordkey, sa => sa) : new Dictionary<string, Data.Student.DataContracts.StudentAdvisement>();

                foreach (var advisorId in advisorIds)
                {
                    try
                    {
                        // Get person record from the personData and start building the advisor.
                        Data.Base.DataContracts.Person personRecord = null;
                        if (personDict.ContainsKey(advisorId))
                        {
                            personRecord = personDict[advisorId];
                        }
                        else
                        {
                            throw new KeyNotFoundException("Person record not found for id " + advisorId);
                        }
                        var advisor = new Advisor(personRecord.Recordkey, personRecord.LastName);
                        advisor.FirstName = personRecord.FirstName;
                        advisor.MiddleName = personRecord.MiddleName;
                        var middleInitial = string.IsNullOrEmpty(personRecord.MiddleName) ? string.Empty : personRecord.MiddleName.Substring(0, 1) + ".";
                        advisor.Name = PersonNameService.FormatName(string.Empty, personRecord.FirstName, middleInitial, personRecord.LastName, string.Empty);
                        if (personRecord.PeopleEmailEntityAssociation != null)
                        {
                            foreach (var personRecordEmail in personRecord.PeopleEmailEntityAssociation)
                            {
                                try
                                {
                                    var email = new EmailAddress(personRecordEmail.PersonEmailAddressesAssocMember, personRecordEmail.PersonEmailTypesAssocMember);
                                    email.IsPreferred = !string.IsNullOrEmpty(personRecordEmail.PersonPreferredEmailAssocMember) && personRecordEmail.PersonPreferredEmailAssocMember.ToUpper() == "Y";
                                    advisor.AddEmailAddress(email);
                                }
                                catch (Exception ex)
                                {
                                    logger.Error("Error occurred while trying to build advisor email: " + ex.Message);
                                }
                            }
                        }

                        // Get the Faculty record from facultyData
                        Student.DataContracts.Faculty facultyRecord = null;
                        if (facultyDict.ContainsKey(advisorId))
                        {
                            facultyRecord = facultyDict[advisorId];
                            // If this faculty is flagged as an advisor, set advisor as "active" and add assigned advisees, if any
                            if (!string.IsNullOrEmpty(facultyRecord.FacAdviseFlag) && facultyRecord.FacAdviseFlag.ToUpper() == "Y")
                            {
                                advisor.IsActive = true;
                                if (facultyRecord.FacAdvisees != null)
                                {
                                    foreach (var studentAdvisementId in facultyRecord.FacAdvisees)
                                    {
                                        Data.Student.DataContracts.StudentAdvisement studentAdvisementRecord = null;
                                        if (studentAdvisementDict.ContainsKey(studentAdvisementId))
                                        {
                                            studentAdvisementRecord = studentAdvisementDict[studentAdvisementId];
                                            try
                                            {
                                                switch (adviseeInclusionType)
                                                {
                                                    case AdviseeInclusionType.AllAdvisees:
                                                        advisor.AddAdvisee(studentAdvisementRecord.StadStudent);
                                                        break;
                                                    case AdviseeInclusionType.ExcludeFormerAdvisees:
                                                        if (!studentAdvisementRecord.StadEndDate.HasValue || studentAdvisementRecord.StadEndDate > DateTime.Today)
                                                        {
                                                            advisor.AddAdvisee(studentAdvisementRecord.StadStudent);
                                                        }
                                                        break;
                                                    case AdviseeInclusionType.CurrentAdviseesOnly:
                                                        if ((!studentAdvisementRecord.StadEndDate.HasValue || studentAdvisementRecord.StadEndDate > DateTime.Today) && 
                                                                (!studentAdvisementRecord.StadStartDate.HasValue || studentAdvisementRecord.StadStartDate <= DateTime.Today))
                                                        {
                                                            advisor.AddAdvisee(studentAdvisementRecord.StadStudent);
                                                        }
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                logger.Error(ex.Message);
                                            }
                                        }
                                        else
                                        {
                                            logger.Error("No student advisement record found for student advisement id " + studentAdvisementId + " for Faculty " + advisorId);
                                        }
                                    }
                                }
                            }
                        }

                        // If advisor is not an active faculty advisor, make sure the person is a staff member. Flag as Active if Current staff.
                        Base.DataContracts.Staff staffRecord = null;
                        if (!advisor.IsActive)
                        {
                            if (staffDict.ContainsKey(advisorId))
                            {
                                staffRecord = staffDict[advisorId];
                                // If this is a current (active) staff, flag as an active advisor
                                if (await IsActiveStaffAsync(staffRecord))
                                {
                                    advisor.IsActive = true;
                                }
                            }
                        }

                        // Add build advisor to the list of advisors to return but only if it's a faculty or staff.
                        if (facultyRecord != null || (await IsStaffAsync(staffRecord)))
                        {
                            advisors.Add(advisor);
                        }
                        else
                        {
                            // If neither faculty or staff, do not add to the returned list. And log the information.
                            logger.Error("Given advisor Id " + advisorId + " is neither faculty nor staff. Returning no data for this Id.");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error occurred while trying to build advisor entity for ID " + advisorId + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred in BuildAdvisor. " + ex.Message);
            }

            return advisors;
        }

        // Returns true if staff data is for a staff person. May throw errors if valcodes not found.
        private async Task<bool> IsStaffAsync(Data.Base.DataContracts.Staff staffRecord)
        {
            if (staffRecord != null)
            {
                var staffTypesValidationTable = new ApplValcodes();
                try
                {
                    // Get the STAFF.TYPES valcode, verify that this is staff record contains a "Staff" type. If not, throw an error.
                    //cache key StaffTypesAsync will be modified to StaffTypes after StaffRepository in base module is made async/
                    staffTypesValidationTable = await GetOrAddToCacheAsync<ApplValcodes>("StaffTypesAsync",
                        async () =>
                        {
                            ApplValcodes staffTypesValTable = await DataReader.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "STAFF.TYPES");
                            if (staffTypesValTable == null)
                            {
                                logger.Error("STAFF.TYPES validation table data is null.");
                                throw new Exception();
                            }
                            return staffTypesValTable;
                        }, Level1CacheTimeoutValue);
                }
                catch (Exception)
                {
                    logger.Error("Unable to retrieve STAFF.TYPES validation table from Colleague.");
                    return false;
                }

                try
                {
                    // If we have a valcode and a nonblank StaffType, find it and check the special processing code to make sure this is a "Staff" type
                    ApplValcodesVals staffType = null;
                    if (staffTypesValidationTable != null && staffTypesValidationTable.ValsEntityAssociation != null && !string.IsNullOrEmpty(staffRecord.StaffType))
                    {
                        staffType = staffTypesValidationTable.ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == staffRecord.StaffType).First();
                    }
                    // We have a staff type entry and it has an action code of S, this is a staffperson.
                    if (staffType != null && !string.IsNullOrEmpty(staffType.ValActionCode1AssocMember) && staffType.ValActionCode1AssocMember.ToUpper() == "S")
                    {
                        return true;
                    }
                    else
                    {
                        logger.Error("Requested staff " + staffRecord.Recordkey + " does not have a valid Staff type.");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error attempting to parse STAFF.TYPES for staff " + staffRecord.Recordkey + " type " + staffRecord.StaffType + " Exception: " + ex.Message);
                }

            }
            return false;
        }

        // Returns a boolean indicating if this is an active staff. The staff must have a status that
        // has an entry in STAFF.STATUSES with a special processing code of "A" for active. Logs error
        // but does not cancel if valcode not found.
        private async Task<bool> IsActiveStaffAsync(Base.DataContracts.Staff staffRecord)
        {
            if (await IsStaffAsync(staffRecord))
            {
                // Get the STAFF.STATUSES valcode 
                var staffStatusesValidationTable = new ApplValcodes();
                try
                {
                    //key will be modified to StaffStatuses after staffRepository in base module is made async
                    staffStatusesValidationTable = await GetOrAddToCacheAsync<ApplValcodes>("StaffStatusesAsync",
                        async () =>
                        {
                            ApplValcodes staffStatusesValTable = await DataReader.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "STAFF.STATUSES");
                            if (staffStatusesValTable == null)
                            {
                                logger.Error("STAFF.STATUSES validation table data is null.");
                                throw new Exception();
                            }
                            return staffStatusesValTable;
                        }, Level1CacheTimeoutValue);
                }
                catch (Exception)
                {
                    // log the issue and move on. Not likely to happen...
                    logger.Error("Unable to retrieve STAFF.STATUSES validation table from Colleague.");
                }

                try
                {
                    // update IsActive flag on advisor record if status indicates Active
                    ApplValcodesVals staffStatus = null;
                    if (staffStatusesValidationTable != null && staffStatusesValidationTable.ValsEntityAssociation != null && !string.IsNullOrEmpty(staffRecord.StaffStatus))
                    {
                        staffStatus = staffStatusesValidationTable.ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == staffRecord.StaffStatus).First();
                    }

                    // Status must have a special processing code of "A" to be considered "Active"
                    if (staffStatus != null && !string.IsNullOrEmpty(staffStatus.ValActionCode1AssocMember) && staffStatus.ValActionCode1AssocMember.ToUpper() == "A")
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error attempting to parse STAFF.STATUS valcode for staff " + staffRecord.Recordkey + " status " + staffRecord.StaffStatus + " Exception: " + ex.Message);
                }
            }
            return false;
        }

        /// <summary>
        /// Finds advisors given a last, first, middle name. First selects PERSON by comparing values against
        /// PERSON.SORT.NAME and first name against nickname. Then limits by selecting person list of ids against FACULTY with advisor set to "Y".
        /// </summary>
        /// <param name="lastName"></param>
        /// <param name="firstName"></param>
        /// <param name="middleName"></param>
        /// <returns>list of Advisor Ids</returns>
        public async Task<IEnumerable<string>> SearchAdvisorByNameAsync(string lastName, string firstName = null, string middleName = null)
        {
            if (string.IsNullOrEmpty(lastName) && string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(middleName))
            {
                return new List<string>();
            }

            var watch = new Stopwatch();
            watch.Start();

            // Search PERSON using the given last, first, middle names
            var advisorIds =await base.SearchByNameAsync(lastName, firstName, middleName);

            logger.Info("Advisor SearchByName(base)... completed in " + watch.ElapsedMilliseconds.ToString());

            watch.Restart();

            // Filter to only return faculty that have advisor set to "Y"

            advisorIds = await  base.FilterByEntityAsync("FACULTY", advisorIds, "WITH FAC.ADVISE.FLAG = 'Y'");

            watch.Stop();

            logger.Info("Advisor FilterByEntity(base)... completed in " + watch.ElapsedMilliseconds.ToString());

            if (advisorIds != null)
            {
                logger.Info("Filtered PERSONS to " + advisorIds.Count() + " FACULTY.");
            }


            return advisorIds;
        }

        /// <summary>
        /// Finds advisors given a last, first, middle name. First selects PERSON by comparing values against
        /// PERSON.SORT.NAME and first name against nickname. Then limits by selecting person list of ids against FACULTY with advisor set to "Y".
        /// </summary>
        /// <param name="lastName"></param>
        /// <param name="firstName"></param>
        /// <param name="middleName"></param>
        /// <returns>list of Advisor Ids</returns>
        public async Task<IEnumerable<string>> SearchAdvisorByNameForExactMatchAsync(string lastName, string firstName = null, string middleName = null)
        {
            if (string.IsNullOrEmpty(lastName) && string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(middleName))
            {
                return new List<string>();
            }

            var watch = new Stopwatch();
            watch.Start();

            // Search PERSON using the given last, first, middle names
            var advisorIds = await base.SearchByNameForExactMatchAsync(lastName, firstName, middleName);

            logger.Info("Advisor SearchByName(base)... completed in " + watch.ElapsedMilliseconds.ToString());

            watch.Restart();

            // Filter to only return faculty that have advisor set to "Y"

            advisorIds = await base.FilterByEntityAsync("FACULTY", advisorIds, "WITH FAC.ADVISE.FLAG = 'Y'");

            watch.Stop();

            logger.Info("Advisor FilterByEntity(base)... completed in " + watch.ElapsedMilliseconds.ToString());

            if (advisorIds != null)
            {
                logger.Info("Filtered PERSONS to " + advisorIds.Count() + " FACULTY.");
            }


            return advisorIds;
        }

    }
}
