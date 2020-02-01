// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentCohortAssignmentsRepository : BaseColleagueRepository, IStudentCohortAssignmentsRepository
    {
        private int readSize;

        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public StudentCohortAssignmentsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            readSize = settings.BulkReadSize;
        }

        /// <summary>
        /// Return collection of student-cohort-assignments
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="criteriaObj"></param>
        /// <param name="filterQualifiers"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<StudentCohortAssignment>, int>> GetStudentCohortAssignmentsAsync(int offset, int limit,
            StudentCohortAssignment criteriaObj = null, Dictionary<string, string> filterQualifiers = null)
        {
            string criteria = string.Empty;
            int totalCount = 0;

            string convertedStartOn = string.Empty;
            string convertedEndOn = string.Empty;

            string[] limitingKeys = new string[] { };
            string[] otherCohortKeys = new string[] { };
            string[] fedCohortKeys = new string[] { };
            string[] combinedCohortKeys = new string[] { };

            List<StudentCohortAssignment> entities = new List<StudentCohortAssignment>();
            List<StudentAcadLevels> studentAcadLevels = null;

            try
            {
                string startOnOperation = filterQualifiers != null && filterQualifiers.ContainsKey("StartOn") ? filterQualifiers["StartOn"] : "EQ";
                string endOnOperation = filterQualifiers != null && filterQualifiers.ContainsKey("EndOn") ? filterQualifiers["EndOn"] : "EQ";
                bool isGetAll = ((criteriaObj == null) || ( string.IsNullOrWhiteSpace(criteriaObj.CohortId) && string.IsNullOrWhiteSpace(criteriaObj.PersonId) && 
                                 !criteriaObj.StartOn.HasValue && !criteriaObj.EndOn.HasValue)) ? 
                                 true : false;

                if (criteriaObj != null && !string.IsNullOrWhiteSpace(criteriaObj.PersonId))
                {
                    criteria = string.Format("WITH STU.ACAD.LEVELS NE '' AND STUDENTS.ID EQ '{0}'", criteriaObj.PersonId);
                    var studentRecords = await DataReader.BulkReadRecordAsync<Students>(criteria);
                    if(studentRecords == null)
                    {
                        return new Tuple<IEnumerable<StudentCohortAssignment>, int>(new List<StudentCohortAssignment>(), 0);
                    }
                    limitingKeys = studentRecords.SelectMany(s => s.StuAcadLevels).Select(al => string.Concat(criteriaObj.PersonId, "*", al)).ToArray();

                    //Get all student acad level records to collect all fed & other cohorts.
                    studentAcadLevels = (await DataReader.BulkReadRecordAsync<StudentAcadLevels>(limitingKeys)).ToList();

                    if(studentAcadLevels == null || !studentAcadLevels.Any())
                    {
                        return new Tuple<IEnumerable<StudentCohortAssignment>, int>(new List<StudentCohortAssignment>(), 0);
                    }

                    //Collect all fed cohort keys
                    fedCohortKeys = studentAcadLevels.Where(sal => !string.IsNullOrEmpty(sal.StaFedCohortGroup))
                                             .Select(fed => string.Concat(fed.Recordkey, "|", fed.StaFedCohortGroup)).ToArray();
                    //Collect other cohorts keys
                    var acadLevels = studentAcadLevels.Where(sal => sal.StaOtherCohortsEntityAssociation != null && sal.StaOtherCohortsEntityAssociation.Any()).ToList();
                    List<string> tempCohortKeys = new List<string>();
                    foreach (var acadLevel in acadLevels)
                    {
                        var id = acadLevel.Recordkey;
                        var otherCohorts = acadLevel.StaOtherCohortsEntityAssociation
                            .Where(oc => HasValues(oc, oc.StaOtherCohortStartDatesAssocMember))
                            .Select(al => string.Concat( id, "|", al.StaOtherCohortGroupsAssocMember, "*", DmiString.DateTimeToPickDate(al.StaOtherCohortStartDatesAssocMember.Value)));
                        tempCohortKeys.AddRange(otherCohorts);                                                    
                    }
                    otherCohortKeys = tempCohortKeys.ToArray();

                    combinedCohortKeys = fedCohortKeys.Concat(otherCohortKeys).ToArray();
                    if (combinedCohortKeys == null || !combinedCohortKeys.Any())
                    {
                        return new Tuple<IEnumerable<StudentCohortAssignment>, int>(new List<StudentCohortAssignment>(), 0);
                    }
                }

                if (criteriaObj != null && !string.IsNullOrWhiteSpace(criteriaObj.CohortId) && !string.IsNullOrWhiteSpace(criteriaObj.CohortType))
                {
                    if (criteriaObj.CohortType.Equals("FED", StringComparison.OrdinalIgnoreCase))
                    {
                        if (limitingKeys == null || !limitingKeys.Any())
                        {
                            criteria = string.Format("WITH STA.FED.COHORT.GROUP EQ '{0}'", criteriaObj.CohortId);
                            studentAcadLevels = (await DataReader.BulkReadRecordAsync<StudentAcadLevels>(criteria)).ToList();
                        }
                        else
                        {
                            studentAcadLevels = studentAcadLevels.Where(sal => sal.StaFedCohortGroup.Equals(criteriaObj.CohortId, StringComparison.OrdinalIgnoreCase)).ToList();
                        }

                        if (studentAcadLevels == null || !studentAcadLevels.Any())
                        {
                            return new Tuple<IEnumerable<StudentCohortAssignment>, int>(new List<StudentCohortAssignment>(), 0);
                        }

                        limitingKeys = studentAcadLevels.Select(lk => lk.Recordkey).ToArray();

                        //Collect all fed cohort keys
                        fedCohortKeys = studentAcadLevels.Where(sal => !string.IsNullOrEmpty(sal.StaFedCohortGroup))
                                                 .Select(fed => string.Concat(fed.Recordkey, "|", fed.StaFedCohortGroup)).ToArray();
                        combinedCohortKeys = fedCohortKeys;
                    }
                    else if (criteriaObj.CohortType.Equals("INSTITUTION", StringComparison.OrdinalIgnoreCase))
                    {
                        if (limitingKeys == null || !limitingKeys.Any())
                        {
                            criteria = string.Format("WITH STA.OTHER.COHORT.GROUPS EQ '{0}' WITH STA.OTHER.COHORT.START.DATES NE ''", criteriaObj.CohortId);
                            studentAcadLevels = (await DataReader.BulkReadRecordAsync<StudentAcadLevels>(criteria)).ToList();
                        }
                        else
                        {
                            studentAcadLevels = studentAcadLevels.Where(sal => sal.StaOtherCohortGroups.Contains(criteriaObj.CohortId)).ToList();
                        }

                        if (studentAcadLevels == null || !studentAcadLevels.Any())
                        {
                            return new Tuple<IEnumerable<StudentCohortAssignment>, int>(new List<StudentCohortAssignment>(), 0);
                        }

                        limitingKeys = studentAcadLevels.Select(lk => lk.Recordkey).ToArray();

                        //Collect other cohorts keys
                        var acadLevels = studentAcadLevels.Where(sal => sal.StaOtherCohortsEntityAssociation != null && sal.StaOtherCohortsEntityAssociation.Any()).ToList();
                        List<string> tempCohortKeys = new List<string>();
                        foreach (var acadLevel in acadLevels)
                        {
                            var id = acadLevel.Recordkey;
                            var otherCohorts = acadLevel.StaOtherCohortsEntityAssociation
                                .Where(oc => HasCohortIdMatch(criteriaObj, oc))
                                .Select(al => string.Concat(id, "|", al.StaOtherCohortGroupsAssocMember, "*", DmiString.DateTimeToPickDate(al.StaOtherCohortStartDatesAssocMember.Value)));
                            tempCohortKeys.AddRange(otherCohorts);
                        }
                        combinedCohortKeys = tempCohortKeys.ToArray();
                    }
                    else
                    {
                        return new Tuple<IEnumerable<StudentCohortAssignment>, int>(new List<StudentCohortAssignment>(), 0);
                    }
                    if (combinedCohortKeys == null || !combinedCohortKeys.Any())
                    {
                        return new Tuple<IEnumerable<StudentCohortAssignment>, int>(new List<StudentCohortAssignment>(), 0);
                    }
                }

                if (criteriaObj != null && criteriaObj.StartOn.HasValue)
                {
                    convertedStartOn = await this.GetUnidataFormattedDateAsync(criteriaObj.StartOn.Value.ToString());
                    if (limitingKeys == null || !limitingKeys.Any())
                    {
                        criteria = string.Format("WITH STA.OTHER.COHORT.START.DATES {0} '{1}' AND STA.OTHER.COHORT.START.DATES NE ''", startOnOperation, convertedStartOn);
                        studentAcadLevels = (await DataReader.BulkReadRecordAsync<StudentAcadLevels>(criteria)).ToList();
                    }
                    else
                    {
                        studentAcadLevels = studentAcadLevels.Where(sal => sal.StaOtherCohortStartDates.Contains(criteriaObj.StartOn.Value)).ToList();
                    }

                    if (studentAcadLevels == null || !studentAcadLevels.Any())
                    {
                        return new Tuple<IEnumerable<StudentCohortAssignment>, int>(new List<StudentCohortAssignment>(), 0);
                    }

                    limitingKeys = studentAcadLevels.Select(lk => lk.Recordkey).ToArray();

                    //Collect other cohorts keys
                    var acadLevels = studentAcadLevels.Where(sal => sal.StaOtherCohortsEntityAssociation != null && sal.StaOtherCohortsEntityAssociation.Any()).ToList();
                    List<string> tempCohortKeys = new List<string>();
                    foreach (var acadLevel in acadLevels)
                    {
                        var id = acadLevel.Recordkey;
                        var otherCohorts = acadLevel.StaOtherCohortsEntityAssociation
                            .Where(oc => HasStartOnMatch(criteriaObj, oc, startOnOperation))
                            .Select(al => string.Concat(id, "|", al.StaOtherCohortGroupsAssocMember, "*", DmiString.DateTimeToPickDate(al.StaOtherCohortStartDatesAssocMember.Value)));
                        tempCohortKeys.AddRange(otherCohorts);
                    }
                    combinedCohortKeys = tempCohortKeys.ToArray();

                    if (combinedCohortKeys == null || !combinedCohortKeys.Any())
                    {
                        return new Tuple<IEnumerable<StudentCohortAssignment>, int>(new List<StudentCohortAssignment>(), 0);
                    }
                }

                if (criteriaObj != null && criteriaObj.EndOn.HasValue)
                {
                    convertedEndOn = await this.GetUnidataFormattedDateAsync(criteriaObj.EndOn.Value.ToString());
                    if (limitingKeys == null || !limitingKeys.Any())
                    {
                        criteria = string.Format("WITH STA.OTHER.COHORT.END.DATES {0} '{1}' AND STA.OTHER.COHORT.END.DATES NE ''", endOnOperation, convertedEndOn);
                        studentAcadLevels = (await DataReader.BulkReadRecordAsync<StudentAcadLevels>(criteria)).ToList();
                    }
                    else
                    {
                        studentAcadLevels = studentAcadLevels.Where(sal => sal.StaOtherCohortEndDates.Contains(criteriaObj.EndOn.Value)).ToList();
                    }

                    if (studentAcadLevels == null || !studentAcadLevels.Any())
                    {
                        return new Tuple<IEnumerable<StudentCohortAssignment>, int>(new List<StudentCohortAssignment>(), 0);
                    }

                    limitingKeys = studentAcadLevels.Select(lk => lk.Recordkey).ToArray();

                    //Collect other cohorts keys
                    var acadLevels = studentAcadLevels.Where(sal => sal.StaOtherCohortsEntityAssociation != null && sal.StaOtherCohortsEntityAssociation.Any()).ToList();
                    List<string> tempCohortKeys = new List<string>();
                    foreach (var acadLevel in acadLevels)
                    {
                        var id = acadLevel.Recordkey;
                        var otherCohorts = acadLevel.StaOtherCohortsEntityAssociation
                            .Where(oc => HasEndOnMatch(criteriaObj, oc, endOnOperation))
                            .Select(al => string.Concat(id, "|", al.StaOtherCohortGroupsAssocMember, "*", DmiString.DateTimeToPickDate(al.StaOtherCohortStartDatesAssocMember.Value)));
                        tempCohortKeys.AddRange(otherCohorts);
                    }
                    combinedCohortKeys = tempCohortKeys.ToArray();

                    if (combinedCohortKeys == null || !combinedCohortKeys.Any())
                    {
                        return new Tuple<IEnumerable<StudentCohortAssignment>, int>(new List<StudentCohortAssignment>(), 0);
                    }
                }

                //GET ALL
                if (isGetAll)
                {
                    criteria = "WITH STU.ACAD.LEVELS NE ''";
                    var studentIds = await DataReader.SelectAsync("STUDENTS", criteria);
                    List<Students> studentRecords = new List<Students>();
                    for (int i = 0; i < studentIds.Count(); i += readSize)
                    {
                        var subListStudents = studentIds.Skip(i).Take(readSize).ToArray();
                        studentRecords.AddRange(await DataReader.BulkReadRecordAsync<Students>(subListStudents));
                    }

                    if (studentRecords == null)
                    {
                        return new Tuple<IEnumerable<StudentCohortAssignment>, int>(new List<StudentCohortAssignment>(), 0);
                    }

                    var tempLimitingKeys = new List<string>();
                    foreach (var studentRecord in studentRecords)
                    {
                        studentRecord.StuAcadLevels.Where(al => !string.IsNullOrEmpty(al)).ToList().ForEach(l =>
                        {
                            var combinedKey = string.Concat(studentRecord.Recordkey, "*", l);
                            tempLimitingKeys.Add(combinedKey);
                        });
                    }
                    limitingKeys = tempLimitingKeys.ToArray();
                    //Get all student acad level records to collect all fed & other cohorts.
                    studentAcadLevels = (await DataReader.BulkReadRecordAsync<StudentAcadLevels>(limitingKeys)).ToList();

                    fedCohortKeys = studentAcadLevels.Where(sal => !string.IsNullOrEmpty(sal.StaFedCohortGroup)).Select(k => string.Concat(k.Recordkey, "|", k.StaFedCohortGroup)).ToArray();

                    //Collect other cohorts keys
                    var acadLevels = studentAcadLevels.Where(sal => sal.StaOtherCohortsEntityAssociation != null && sal.StaOtherCohortsEntityAssociation.Any()).ToList();
                    List<string> tempCohortKeys = new List<string>();
                    foreach (var acadLevel in acadLevels)
                    {
                        var id = acadLevel.Recordkey;
                        var otherCohorts = acadLevel.StaOtherCohortsEntityAssociation
                            .Where(oc => HasValues(oc, oc.StaOtherCohortStartDatesAssocMember))
                            .Select(al => string.Concat(id, "|", al.StaOtherCohortGroupsAssocMember, "*", DmiString.DateTimeToPickDate(al.StaOtherCohortStartDatesAssocMember.Value)));
                        tempCohortKeys.AddRange(otherCohorts);
                    }
                    otherCohortKeys = tempCohortKeys.ToArray();

                    combinedCohortKeys = fedCohortKeys.Concat(otherCohortKeys).ToArray();
                    if (combinedCohortKeys == null || !combinedCohortKeys.Any())
                    {
                        return new Tuple<IEnumerable<StudentCohortAssignment>, int>(new List<StudentCohortAssignment>(), 0);
                    }
                }

                //Get total count from combinedCohortKeys
                totalCount = combinedCohortKeys.Count();
                var subList = combinedCohortKeys.Skip(offset).Take(limit).ToArray();
                var subListIds = subList.Select(i => i.Split('|')[0]).Distinct().ToArray();                

                Dictionary<string, string> dict = await GetGuidsCollectionAsync(subList);

                foreach (var recordId in subList)
                {
                    var splitKey = recordId.Split('|');
                    var recKey = recordId.Split('|')[0];
                    var cohKey = recordId.Split('|')[1];
                    var personId = string.Empty;
                    var acadLevelId = string.Empty;
                    var acadLevel = studentAcadLevels.FirstOrDefault(fc => fc.Recordkey.Equals(recKey, StringComparison.OrdinalIgnoreCase) );

                    if (acadLevel != null)
                    {
                        personId = acadLevel.Recordkey.Split('*')[0];
                        acadLevelId = recKey.Split('*')[1];
                        if (!string.IsNullOrEmpty(acadLevel.StaFedCohortGroup) && acadLevel.StaFedCohortGroup.Equals(cohKey, StringComparison.OrdinalIgnoreCase))
                        {
                            var guid = string.Empty;
                            if (dict.TryGetValue(string.Concat(acadLevel.Recordkey, "|", acadLevel.StaFedCohortGroup), out guid))
                            {
                                StudentCohortAssignment fedEntity = new StudentCohortAssignment(acadLevel.StaFedCohortGroup, guid)
                                {
                                    PersonId = personId,
                                    AcadLevel = acadLevelId,
                                    CohortId = acadLevel.StaFedCohortGroup
                                };
                                entities.Add(fedEntity);
                            }
                        }
                    }

                    if (cohKey.Contains("*"))
                    {
                        var splitCohKey = cohKey.Split('*');
                        var othCohKey = splitCohKey[0];
                        var startDate = DmiString.PickDateToDateTime(Convert.ToInt32(splitCohKey[1]));
                        var otherAcadLevel = acadLevel.StaOtherCohortsEntityAssociation.FirstOrDefault(i => i.StaOtherCohortGroupsAssocMember.Equals(othCohKey) &&
                                                                                                        i.StaOtherCohortStartDatesAssocMember.Value.Date.Equals(startDate));
                        if (otherAcadLevel != null)
                        {
                            personId = acadLevel.Recordkey.Split('*')[0];

                            var guid = string.Empty;
                            if (dict.TryGetValue(string.Concat(acadLevel.Recordkey, "|", cohKey), out guid))
                            {
                                StudentCohortAssignment otherEntity = new StudentCohortAssignment(othCohKey, guid)
                                {
                                    CohortId = othCohKey,
                                    PersonId = personId,
                                    AcadLevel = acadLevelId,
                                    StartOn = otherAcadLevel.StaOtherCohortStartDatesAssocMember,
                                    EndOn = otherAcadLevel.StaOtherCohortEndDatesAssocMember
                                };
                                entities.Add(otherEntity);
                            }
                        }
                    }
                }                                    
            }
            catch (RepositoryException e)
            {
                throw e;
            }
            return entities.Any()? new Tuple<IEnumerable<StudentCohortAssignment>, int>(entities, totalCount) :
                new Tuple<IEnumerable<StudentCohortAssignment>, int>(new List<StudentCohortAssignment>(), 0);
        }
        private bool HasValues(StudentAcadLevelsStaOtherCohorts oc, DateTime? date)
        {
            return !string.IsNullOrEmpty(oc.StaOtherCohortGroupsAssocMember) && date.HasValue;
        }

        private bool HasCohortIdMatch(StudentCohortAssignment criteriaObj, StudentAcadLevelsStaOtherCohorts oc)
        {
            return HasValues(oc, oc.StaOtherCohortStartDatesAssocMember) && oc.StaOtherCohortGroupsAssocMember.Equals(criteriaObj.CohortId, StringComparison.OrdinalIgnoreCase);
        }

        private bool HasStartOnMatch(StudentCohortAssignment criteriaObj, StudentAcadLevelsStaOtherCohorts oc, string startOnOperation)
        {
            return HasValues(oc, oc.StaOtherCohortStartDatesAssocMember) && IsDateCriteriaMatch(DmiString.DateTimeToPickDate(oc.StaOtherCohortStartDatesAssocMember.Value),
                   DmiString.DateTimeToPickDate(criteriaObj.StartOn.Value), startOnOperation);
        }

        private bool HasEndOnMatch(StudentCohortAssignment criteriaObj, StudentAcadLevelsStaOtherCohorts oc, string endOnOperation)
        {
            return HasValues(oc, oc.StaOtherCohortEndDatesAssocMember) && IsDateCriteriaMatch(DmiString.DateTimeToPickDate(oc.StaOtherCohortEndDatesAssocMember.Value),
                   DmiString.DateTimeToPickDate(criteriaObj.EndOn.Value), endOnOperation);
        }


        /// <summary>
        /// Returns single student-cohort-assignment
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<StudentCohortAssignment> GetStudentCohortAssignmentByIdAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var ldmEntity = await DataReader.ReadRecordAsync<LdmGuid>("LDM.GUID", guid);
            if (ldmEntity == null)
            {
                throw new KeyNotFoundException("STUDENT.ACAD.LEVELS '" + guid + "' not found.");
            }
            if ((ldmEntity.LdmGuidEntity != "STUDENT.ACAD.LEVELS") && (ldmEntity.LdmGuidSecondaryFld != "STA.OTHER.COHORTS.IDX" || ldmEntity.LdmGuidSecondaryFld != "STA.FED.COHORT.GROUP"))
            {
                throw new KeyNotFoundException("GUID '" + guid + "' is invalid.  Expecting GUID with entity STUDENT.ACAD.LEVELS with a secondary field equal to STA.OTHER.COHORTS.IDX or STA.FED.COHORT.GROUP.");
            }

            var entityRec = await DataReader.ReadRecordAsync<StudentAcadLevels>(ldmEntity.LdmGuidPrimaryKey);
            if(entityRec == null)
            {
                throw new KeyNotFoundException("GUID " + guid + " is invalid.  Expecting GUID with entity STUDENT.ACAD.LEVELS with a secondary field equal to STA.OTHER.COHORTS.IDX or STA.FED.COHORT.GROUP.");
            }
            StudentCohortAssignment entity = null;
            Dictionary<string, string> dict = await this.GetGuidsCollectionAsync(new List<string>() { string.Concat(ldmEntity.LdmGuidPrimaryKey, "|", ldmEntity.LdmGuidSecondaryKey) });
            if (ldmEntity.LdmGuidSecondaryFld.Equals("STA.FED.COHORT.GROUP", StringComparison.OrdinalIgnoreCase))
            {
                var recGuid = string.Empty;
                if(!dict.TryGetValue(string.Concat(entityRec.Recordkey, "|", ldmEntity.LdmGuidSecondaryKey), out recGuid))
                {
                    throw new KeyNotFoundException("GUID '" + guid + "' is invalid.  Expecting GUID with entity STUDENT.ACAD.LEVELS with a secondary field equal to STA.OTHER.COHORTS.IDX or STA.FED.COHORT.GROUP.");
                }


                entity = new StudentCohortAssignment(entityRec.StaFedCohortGroup, recGuid);
                entity.PersonId = entityRec.Recordkey.Split('*')[0];
                entity.AcadLevel = entityRec.Recordkey.Split('*')[1];
                entity.CohortId = entityRec.StaFedCohortGroup;
            }
            else if(ldmEntity.LdmGuidSecondaryFld.Equals("STA.OTHER.COHORTS.IDX", StringComparison.OrdinalIgnoreCase))
            {
                if(entityRec.StaOtherCohortsEntityAssociation != null && entityRec.StaOtherCohortsEntityAssociation.Any())
                {
                    var date = DmiString.PickDateToDateTime(Convert.ToInt32(ldmEntity.LdmGuidSecondaryKey.Split('*')[1]));
                    var record = entityRec.StaOtherCohortsEntityAssociation
                                       .FirstOrDefault(oc => oc.StaOtherCohortGroupsAssocMember.Equals(ldmEntity.LdmGuidSecondaryKey.Split('*')[0], StringComparison.OrdinalIgnoreCase) &&
                                                             oc.StaOtherCohortStartDatesAssocMember.Value.Date.Equals(date));
                    var recGuid = string.Empty;
                    var key = string.Concat(entityRec.Recordkey, "|", record.StaOtherCohortGroupsAssocMember, "*", DmiString.DateTimeToPickDate(record.StaOtherCohortStartDatesAssocMember.Value));
                    if (!dict.TryGetValue(key, out recGuid))
                    {
                        throw new KeyNotFoundException("GUID " + guid + " is invalid.  Expecting GUID with entity STUDENT.ACAD.LEVELS with a secondary field equal to STA.OTHER.COHORTS.IDX or STA.FED.COHORT.GROUP.");
                    }

                    if (record == null)
                    {
                        throw new KeyNotFoundException("GUID " + guid + " is invalid.  Expecting GUID with entity STUDENT.ACAD.LEVELS with a secondary field equal to STA.OTHER.COHORTS.IDX or STA.FED.COHORT.GROUP.");
                    }
                    entity = new StudentCohortAssignment(record.StaOtherCohortGroupsAssocMember, recGuid);
                    entity.PersonId = entityRec.Recordkey.Split('*')[0];
                    entity.AcadLevel = entityRec.Recordkey.Split('*')[1];
                    entity.CohortId = record.StaOtherCohortGroupsAssocMember;
                    entity.StartOn = record.StaOtherCohortStartDatesAssocMember.Value.Date;
                    entity.EndOn = record.StaOtherCohortEndDatesAssocMember.HasValue ? record.StaOtherCohortEndDatesAssocMember.Value.Date : default(DateTime?);
                }
            }
            else
            {
                throw new KeyNotFoundException("GUID '" + guid + "' is invalid.  Expecting GUID with entity STUDENT.ACAD.LEVELS with a secondary field equal to STA.OTHER.COHORTS.IDX or STA.FED.COHORT.GROUP.");
            }

            return entity;
        }

        /// <summary>
        /// Using a collection of ids with guids
        ///  get a dictionary collection of associated guids
        /// </summary>
        /// <param name="ids">collection of ids</param>
        /// <returns>Dictionary consisting of a application.id with guids</returns>
        public async Task<Dictionary<string, string>> GetGuidsCollectionAsync(IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any())
            {
                return new Dictionary<string, string>();
            }
            var guidCollection = new Dictionary<string, string>();
            try
            {
                var guidLookup1 = ids.Select(s => new
                {
                    recordKey = s.Split(new[] { '|' })[0],
                    secondardaryKey = s.Split(new[] { '|' })[1],
                })
                    .Where(s => !string.IsNullOrWhiteSpace(s.recordKey) && !s.secondardaryKey.Contains("*"))
                    .Distinct().ToList()
                    .ConvertAll(applicationKey => new RecordKeyLookup("STUDENT.ACAD.LEVELS", applicationKey.recordKey,
                    "STA.FED.COHORT.GROUP", applicationKey.secondardaryKey, false))
                    .ToArray();

                var guidLookup2 = ids.Select(s => new
                {
                    recordKey = s.Split(new[] { '|' })[0],
                    secondardaryKey = s.Split(new[] { '|' })[1],
                })
                    .Where(s => !string.IsNullOrWhiteSpace(s.recordKey) && s.secondardaryKey.Contains("*"))
                    .Distinct().ToList()
                    .ConvertAll(applicationKey => new RecordKeyLookup("STUDENT.ACAD.LEVELS", applicationKey.recordKey,
                    "STA.OTHER.COHORTS.IDX", applicationKey.secondardaryKey, false))
                    .ToArray();

                var recordKeyLookupResults1 = await DataReader.SelectAsync(guidLookup1);
                var recordKeyLookupResults2 = await DataReader.SelectAsync(guidLookup2);
                var recordKeyLookupResults = recordKeyLookupResults1.Concat(recordKeyLookupResults2);

                if ((recordKeyLookupResults != null) && (recordKeyLookupResults.Any()))
                {
                    foreach (var recordKeyLookupResult in recordKeyLookupResults)
                    {
                        if (recordKeyLookupResult.Value != null)
                        {
                            var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                            if (!guidCollection.ContainsKey(splitKeys[1]))
                            {
                                guidCollection.Add(string.Concat(splitKeys[1], "|", splitKeys[2]), recordKeyLookupResult.Value.Guid);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error occured while getting guids for {0}.", "STUDENT.ACAD.LEVELS"), ex);
            }

            return guidCollection;
        }

        /// <summary>
        /// Check the date criteria match.
        /// </summary>
        /// <param name="oc"></param>
        /// <param name="dmiDate"></param>
        /// <param name="dateOperator"></param>
        /// <returns></returns>
        private bool IsDateCriteriaMatch(int oc, int dmiDate, string dateOperator)
        {
            switch (dateOperator)
            {
                case "NE":
                    return !oc.Equals(dmiDate);
                case "LT":
                case "GT":
                    return false;
                case "LE":
                    return oc <= dmiDate;
                case "GE":
                    return oc >= dmiDate;
                default:
                    return oc.Equals(dmiDate);
            }
            
        }

        /// <summary>
        /// Gets integer part of the key.
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private int GetInt(string num)
        {
            int myOut;
            if (int.TryParse(num, out myOut))
            {
                return myOut;
            }
            return myOut;
        }

        /// <summary>
        /// Return a Unidata Formatted Date string from an input argument of string type
        /// </summary>
        /// <param name="date">String representing a Date</param>
        /// <returns>Unidata formatted Date string for use in Colleague Selection.</returns>
        private async Task<string> GetUnidataFormattedDateAsync(string date)
        {
            var internationalParameters = await GetInternationalParametersAsync();
            var newDate = DateTime.Parse(date).Date;
            return UniDataFormatter.UnidataFormatDate(newDate, internationalParameters.HostShortDateFormat, internationalParameters.HostDateDelimiter);
        }
    }
}