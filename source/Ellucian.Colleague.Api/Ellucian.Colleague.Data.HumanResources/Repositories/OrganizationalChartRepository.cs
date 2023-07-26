/* Copyright 2023 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
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

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class OrganizationalChartRepository : BaseColleagueRepository, IOrganizationalChartRepository
    {
        private readonly int bulkReadSize;

        public OrganizationalChartRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Given a "root" person id, get a single level of parent (supervisors) and child (supervisees) for this person
        /// </summary>
        /// <param name="rootEmployeeId">The employee id to build the org chart off of</param>
        /// <returns>IEnumerable<OrgChartNode></returns>
        public async Task<IEnumerable<OrgChartNode>> GetActiveOrgChartEmployeesAsync(string rootEmployeeId)
        {
            var cacheControlKey = string.Concat(rootEmployeeId, "_OrgChart");
            return await GetOrAddToCacheAsync<IEnumerable<OrgChartNode>>(cacheControlKey,
                   async () =>
                   {
                       var orgChart = new List<OrgChartNode>();

                       // STEP 1: find the given root's primary position (perpos) key and record
                       var primaryPerposId = await GetActivePrimaryPerposIdForPerson(rootEmployeeId);
                       var primaryPerposRecord = await GetPerposRecords(new List<string>() { primaryPerposId });

                       if (primaryPerposRecord != null && primaryPerposRecord.Any())
                       {
                           try
                           {
                               // STEP 2: create an employee node for the requested employee id to be used to get the supervisees (children) for the supervisor
                               var employeeNode = new OrgChartNode();

                               employeeNode = new OrgChartNode(primaryPerposRecord.FirstOrDefault().Recordkey,
                                    primaryPerposRecord.FirstOrDefault().PerposHrpId,
                                    primaryPerposRecord.FirstOrDefault().PerposPositionId, "", "", "", 0);

                               // STEP 3: obtain all children (supervisees) for the current parent node
                               var directSuperviseeNodes = await GetChildrenFromParents(new List<OrgChartNode>() { employeeNode });

                               if (directSuperviseeNodes.Any())
                               {
                                   // STEP 4: if any child nodes were found for the parent
                                   orgChart.AddRange(directSuperviseeNodes);
                                   directSuperviseeNodes.Clear();
                               }
                           }
                           catch (Exception e)
                           {
                               logger.Error(String.Format("Unexpected error creating organizational chart {0}.", e.Message));
                           }
                       }
                       else
                       {
                           logger.Debug(String.Format("Given employee does not have an active primary position {0}.", rootEmployeeId));
                       }

                       return orgChart;
                   }, Level1CacheTimeoutValue);
        }

        /// <summary>
        /// Get the single OrgChartNode for the given "root" employee
        /// </summary>
        /// <param name="rootEmployeeId">The employee id to build the org chart off of</param>
        /// <returns>OrgChartNode</returns>
        public async Task<OrgChartNode> GetActiveOrgChartEmployeeAsync(string rootEmployeeId)
        {
            var employeeNode = new OrgChartNode();

            // STEP 1: find the given root employee's primary position (perpos) key and record
            var primaryPerposId = await GetActivePrimaryPerposIdForPerson(rootEmployeeId);
            var primaryPerposRecord = await GetPerposRecords(new List<string>() { primaryPerposId });

            if (primaryPerposRecord != null && primaryPerposRecord.Any())
            {
                try
                {
                    // STEP 2a: find the number of direct supervisees for this employee
                    var directSupervisees = await GetActiveSupervisedPerposKeys(new List<string>() { rootEmployeeId });
                    var countOfDirectSupervisees = directSupervisees.Count();

                    // STEP 2b: obtain the supervisor from the primary perpos record for the given employee
                    if (!String.IsNullOrEmpty(primaryPerposRecord.FirstOrDefault().PerposSupervisorHrpId))
                    {
                        var supervisorHrpId = primaryPerposRecord.FirstOrDefault().PerposSupervisorHrpId;
                        var supervisorPrimaryPerposId = await GetActivePrimaryPerposIdForPerson(supervisorHrpId);
                        var supervisorPrimaryPerposRecord = await GetPerposRecords(new List<string>() { supervisorPrimaryPerposId });

                        // STEP 2c: return the given employee along with their supervisor information
                        employeeNode = new OrgChartNode(
                            primaryPerposRecord.FirstOrDefault().Recordkey,
                            primaryPerposRecord.FirstOrDefault().PerposHrpId,
                            primaryPerposRecord.FirstOrDefault().PerposPositionId,
                            supervisorPrimaryPerposRecord.FirstOrDefault().Recordkey,
                            supervisorPrimaryPerposRecord.FirstOrDefault().PerposHrpId,
                            supervisorPrimaryPerposRecord.FirstOrDefault().PerposPositionId,
                            countOfDirectSupervisees);
                    }
                    else
                    {
                        // STEP 2c: return the given employee without any supervisor information
                        employeeNode = new OrgChartNode(
                            primaryPerposRecord.FirstOrDefault().Recordkey,
                            primaryPerposRecord.FirstOrDefault().PerposHrpId,
                            primaryPerposRecord.FirstOrDefault().PerposPositionId, "", "", "", countOfDirectSupervisees);
                    }
                }
                catch (Exception e)
                {
                    logger.Error(String.Format("Unexpected error creating organizational chart {0}.", e.Message));
                }
            }
            else
            {
                logger.Debug(String.Format("Given employee does not have an active primary position {0}.", rootEmployeeId));
            }
            return employeeNode;
        }

        /// <summary>
        /// use this method to get a all children for all given parent nodes. for example, a tree starts with 1 single root node.passing the root node to this method
        /// results in (for example) 3 child nodes. calling this method again will return all child nodes for each of the 3 (now) parent nodes.
        /// </summary>
        /// <param name="parentNodes">List of parent OrgChartNode</param>
        /// <returns>List of children OrgChartNotes</returns>
        private async Task<List<OrgChartNode>> GetChildrenFromParents(List<OrgChartNode> parentNodes)
        {
            var childNodes = new List<OrgChartNode>();

            if (parentNodes != null && parentNodes.Any())
            {
                // STEP 1: create a list of all parent ids for all the parent nodes
                var parentEmployeeIds = parentNodes.Select(pn => pn.EmployeeId).ToList();
                if (parentEmployeeIds != null && parentEmployeeIds.Any())
                {
                    // STEP 2: using the list of parent ids, get all child records for all parents; do this to make only one db request for all records at once
                    var childPerposKeys = await GetActiveSupervisedPerposKeys(parentEmployeeIds);
                    var childPerPosRecords = await GetPerposRecords(childPerposKeys);
                    if (childPerPosRecords != null && childPerPosRecords.Any())
                    {
                        // for each parent node, find the (already selected) child perpos records... 
                        foreach (var parentNode in parentNodes)
                        {
                            var selectedChildrenRecords = childPerPosRecords.Where(cppr => cppr.PerposSupervisorHrpId == parentNode.EmployeeId);
                            if (selectedChildrenRecords != null && selectedChildrenRecords.Any())
                            {
                                // for each of the children of this parent, find the primary position id and perpos record...
                                foreach (var selectedChild in selectedChildrenRecords)
                                {
                                    var primaryPerposId = await GetActivePrimaryPerposIdForPerson(selectedChild.PerposHrpId);
                                    if (!String.IsNullOrEmpty(primaryPerposId))
                                    {
                                        var primaryPerposRecord = await GetPerposRecords(new List<string>() { primaryPerposId });
                                        {
                                            var childPrimaryPerposRecod = primaryPerposRecord.FirstOrDefault();
                                            var existingPerposFound = childNodes.Select(cn => cn.PersonPositionId == childPrimaryPerposRecod.Recordkey).FirstOrDefault();
                                            if (!existingPerposFound)
                                            {
                                                var directSupervisees = await GetActiveSupervisedPerposKeys(new List<string>() { childPrimaryPerposRecod.PerposHrpId });
                                                var countOfDirectSupervisees = directSupervisees.Count();
                                                childNodes.Add(new OrgChartNode(childPrimaryPerposRecod.Recordkey,
                                                    childPrimaryPerposRecod.PerposHrpId,
                                                    childPrimaryPerposRecod.PerposPositionId,
                                                    parentNode.PersonPositionId,
                                                    parentNode.EmployeeId,
                                                    parentNode.PositionCode,
                                                    countOfDirectSupervisees));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            }
            return childNodes;
        }

        /// <summary>
        /// Gets PERPOS records 
        /// </summary>
        /// <param name="perposIds"></param>
        /// <returns>List of PERPOS records</returns>
        private async Task<List<Perpos>> GetPerposRecords(IEnumerable<string> perposIds)
        {
            var distinctPerposIds = perposIds.Distinct();
            var returnList = new List<Perpos>();
            for (int i = 0; i < distinctPerposIds.Count(); i += bulkReadSize)
            {
                var subList = distinctPerposIds.Skip(i).Take(bulkReadSize);
                var perposRecords = await DataReader.BulkReadRecordAsync<Perpos>(subList.ToArray());
                if (perposRecords == null)
                {
                    logger.Error("Unexpected null from bulk read of Perpos records");
                }
                else
                {
                    returnList.AddRange(perposRecords);
                }
            }
            return returnList;
        }

        /// <summary>
        /// Gets actively supervised PERPOS keys
        /// </summary>
        /// <param name="supervisorIds">List of supervisor ids</param>
        /// <returns>List of PERPOS keys currently supervised by the supervisors</returns>
        private async Task<IEnumerable<string>> GetActiveSupervisedPerposKeys(List<string> supervisorIds)
        {
            var currentDateFormatted = await GetUnidataFormatDateAsync(DateTime.Now);
            var critiera = "WITH PERPOS.START.DATE LE '" + currentDateFormatted + "' AND (PERPOS.END.DATE GE '" + currentDateFormatted + "' OR PERPOS.END.DATE EQ '')";
            critiera += " AND INDEX.PERPOS.SUPERVISOR EQ ?";
            var criteriaValues = supervisorIds.Distinct().Select(id => string.Format("\"{0}\"", id));

            var activePerposKeys = await DataReader.SelectAsync("PERPOS", critiera, criteriaValues.ToArray());

            if (activePerposKeys == null)
            {
                var message = "Unexpected null returned from PERPOS SelectAsyc";
                logger.Error(message);
                activePerposKeys = new string[0];
            }
            return activePerposKeys;
        }

        /// <summary>
        /// Get Active Primary Perpos Id's by person id
        /// </summary>
        /// <param name="personId">Employee ID</param>
        /// <returns></returns>
        private async Task<string> GetActivePrimaryPerposIdForPerson(string personId)
        {
            var currentDateFormatted = await GetUnidataFormatDateAsync(DateTime.Now);
            var critiera = string.Format("WITH PERSTAT.HRP.ID EQ {0}", personId);
            critiera += " AND PERSTAT.START.DATE LE '" + currentDateFormatted + "' AND (PERSTAT.END.DATE GE '" + currentDateFormatted + "' OR PERSTAT.END.DATE EQ '')";
            critiera += " AND PERSTAT.PRIMARY.PERPOS.ID NE ''";

            var perstatKeys = await DataReader.SelectAsync("PERSTAT", critiera);

            if (!perstatKeys.Any() || perstatKeys == null)
            {
                logger.Debug(string.Format("No PERSTAT keys exist for person: {0} with matching critiera {1}", personId, critiera));
                return null;
            }

            if (perstatKeys.Any() && perstatKeys.Count() > 1)
            {
                logger.Debug(string.Format("Unexpected result from PERSTAT select for person {0} with critiera {1}: person has more than one active primary perpos.", personId, critiera));
                logger.Debug(string.Format("Using the first perstat record id {0}.", perstatKeys.FirstOrDefault()));
            }

            var perstatRecord = await DataReader.ReadRecordAsync<Perstat>(perstatKeys.FirstOrDefault());

            if (perstatRecord == null)
            {
                logger.Debug(string.Format("Unable to read PERSTAT record {0}", perstatKeys.FirstOrDefault()));
                return null;
            }

            if (perstatRecord.PerstatPrimaryPerposId == null)
            {
                logger.Debug(string.Format("Expected PERSTAT.PRIMARY.PERPOS.ID for PERSTAT {0}", perstatKeys.FirstOrDefault()));
                return null;
            }

            return perstatRecord.PerstatPrimaryPerposId;
        }
    }
}