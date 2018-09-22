/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Data.HumanResources.Transactions;
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
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class FacultyContractRepository : BaseColleagueRepository, IFacultyContractRepository
    {
        private readonly int bulkReadSize;
        public FacultyContractRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
        }

        public async Task<IEnumerable<FacultyContract>> GetFacultyContractsByFacultyIdAsync(string facultyId)
        {
            var pacCriteria = "WITH PAC.HRP.ID EQ '?'";
            IEnumerable<string> facultyIds = new List<string>() { facultyId };
            var pacIds = await DataReader.SelectAsync("PER.ASGMT.CONTRACT", pacCriteria, facultyIds.ToArray());
            //var records = await DataReader.BulkReadRecordAsync<PacLoadPeriods>("PAC.LOAD.PERIODS", "");
            //only time you wouldn't use "Skip" is if you know the result will be less than the readSize (you never really know what the read size is)
            List<PerAsgmtContract> pacRecords = new List<PerAsgmtContract>();
            if (pacIds != null && pacIds.Count() > 0)
            {
                for (int i = 0; i < pacIds.Count(); i += bulkReadSize)
                {
                    var subList = pacIds.Skip(i).Take(bulkReadSize).ToArray();
                    var bulkRecords = await DataReader.BulkReadRecordAsync<PerAsgmtContract>(subList);
                    if (bulkRecords != null)
                    {
                        pacRecords.AddRange(bulkRecords);
                    }
                }
            }

            List<PacLoadPeriods> pacLoadPeriodRecords = new List<PacLoadPeriods>();
            var pacLoadPeriodIds = pacRecords.SelectMany(pr => pr.PacAllLoadPeriods);

            if (pacLoadPeriodIds != null && pacLoadPeriodIds.Count() > 0)
            {
                for (int i = 0; i < pacLoadPeriodIds.Count(); i += bulkReadSize)
                {
                    var subList = pacLoadPeriodIds.Skip(i).Take(bulkReadSize).ToArray();
                    var bulkRecords = await DataReader.BulkReadRecordAsync<PacLoadPeriods>(subList);
                    if (bulkRecords != null)
                    {
                        pacLoadPeriodRecords.AddRange(bulkRecords);
                    }
                }
            }

            var pacLpPositionIds = pacLoadPeriodRecords.SelectMany(pr => pr.PlpPacLpPositionIds);
            var pacLpPositions = await GetPacLpPositionsAsync(pacLpPositionIds);

            var pacLpAsgmtIds = pacLpPositions.SelectMany(pr => pr.PlppPacLpAsgmtsIds);
            var pacLpAsgmts = await GetPacLpAsgmtsAsync(pacLpAsgmtIds);



            var assignmentLoadRequest = new GetFacultyAsgmtAmtsRequest();
            foreach (var id in pacLpAsgmtIds)
            {
                assignmentLoadRequest.AssignmentAmounts.Add(new AssignmentAmounts() { PacLpAsgmtsId = id });
            }
            var assignmentLoadResponse = await transactionInvoker.ExecuteAsync<GetFacultyAsgmtAmtsRequest, GetFacultyAsgmtAmtsResponse>(assignmentLoadRequest);
            var assignmentAmounts = assignmentLoadResponse != null ? assignmentLoadResponse.AssignmentAmounts : new List<AssignmentAmounts>();
            List<FacultyContractPosition> positionWithAssignments = CombinePositionAndAssignments(pacLpPositions, pacLpAsgmts, assignmentAmounts);

            List<PacLoadPeriods> visiblePacLoadPeriodRecords = new List<PacLoadPeriods>();

            // Returns the default HRSS rules
            var hrssDefaults = await GetHrssDefaultAsync();
            if (hrssDefaults != null)
            {
                // List of the contract statuses
                var contractStatuses = hrssDefaults.HrssFacContWebVisible;

                //Sorts which PacLoadPeriodRecords have contracts with visible statuses
                visiblePacLoadPeriodRecords = pacLoadPeriodRecords.Where(plp => contractStatuses.Contains(plp.PlpStatuses.First())).ToList();
            }

            List<FacultyContract> facultyContracts = new List<FacultyContract>();

            foreach (var plpRecord in visiblePacLoadPeriodRecords)
            {
                var matchingPacRecord = pacRecords.FirstOrDefault(pr => pr.Recordkey == plpRecord.PlpPerAsgmtContractId);

                if (matchingPacRecord == null)
                {
                    LogDataError("PAC.LOAD.PERIODS", plpRecord.Recordkey, plpRecord);
                }

                FacultyContract facContract = new FacultyContract(plpRecord.Recordkey, matchingPacRecord.PacDesc, matchingPacRecord.PacNo, matchingPacRecord.PacType,
                    matchingPacRecord.PacStartDate, matchingPacRecord.PacEndDate, plpRecord.PlpLoadPeriod, plpRecord.PlpIntendedTotalLoad, plpRecord.PlpTotalValue);


                foreach (var position in positionWithAssignments)
                {
                    // Check to verify FacultyContractPosition matches the PLP.PAC.LP.POSITION.IDS
                    if (plpRecord.PlpPacLpPositionIds.Contains(position.Id))
                    {
                        facContract.FacultyContractPositions.Add(position);
                    }
                }
                facultyContracts.Add(facContract);
            }
            return facultyContracts;
        }

        private List<FacultyContractPosition> CombinePositionAndAssignments(IEnumerable<PacLpPositions> pacLpPositions, IEnumerable<PacLpAsgmts> pacLpAsgmts, IEnumerable<AssignmentAmounts> amounts)
        {
            List<FacultyContractPosition> result = new List<FacultyContractPosition>();
            foreach (var position in pacLpPositions)
            {
                FacultyContractPosition pos = new FacultyContractPosition(position.Recordkey, position.PlppPacLoadPeriodsId, position.PlppIntendedLoad, position.PlppPositionId);
                foreach (var assignment in pacLpAsgmts.Where(pacLpAsgmt => position.PlppPacLpAsgmtsIds.Contains(pacLpAsgmt.Recordkey)))
                {
                    FacultyContractAssignmentType assignmentType;
                    switch (assignment.PlpaAsgmtType)
                    {
                        case "V":
                            assignmentType = FacultyContractAssignmentType.CampusOrganizationAdvisor;
                            break;
                        case "M":
                            assignmentType = FacultyContractAssignmentType.CampusOrganizationMember;
                            break;
                        case "S":
                        default:
                            assignmentType = FacultyContractAssignmentType.CourseSectionFaculty;
                            break;
                    }
                    var asgmtAmountRecord = amounts.FirstOrDefault(a => a.PacLpAsgmtsId.Equals(assignment.Recordkey));
                    string amount;
                    if (asgmtAmountRecord == null)
                    {
                        amount = "0";
                        logger.Error("No amount found for Faculty assignment "+ assignment.Recordkey+". Under user "+ assignment.PlpaHrpId+".");
                    }
                    else
                    {
                        amount = asgmtAmountRecord.AsgmtAmount;
                    }
                    FacultyContractAssignment asgmt = new FacultyContractAssignment(assignment.Recordkey, assignment.PlpaHrpId, assignmentType, assignment.PlpaPacLpPositionsId, assignment.PlpaAsgmtId, amount);
                    pos.FacultyContractAssignments.Add(asgmt);
                }
                result.Add(pos);
            }
            return result;
        }

        private async Task<IEnumerable<PacLpPositions>> GetPacLpPositionsAsync(IEnumerable<string> positionIds)
        {
            List<PacLpPositions> pacLpPositionRecords = new List<PacLpPositions>();
            if (positionIds != null && positionIds.Count() > 0)
            {
                for (int i = 0; i < positionIds.Count(); i += bulkReadSize)
                {
                    var subList = positionIds.Skip(i).Take(bulkReadSize).ToArray();
                    var bulkRecords = await DataReader.BulkReadRecordAsync<PacLpPositions>(subList);
                    if (bulkRecords != null)
                    {
                        pacLpPositionRecords.AddRange(bulkRecords);
                    }
                }
            }
            return pacLpPositionRecords;
        }

        private async Task<IEnumerable<PacLpAsgmts>> GetPacLpAsgmtsAsync(IEnumerable<string> asgmtsIds)
        {
            List<PacLpAsgmts> pacLpAsgmtsRecords = new List<PacLpAsgmts>();
            if (asgmtsIds != null && asgmtsIds.Count() > 0)
            {
                for (int i = 0; i < asgmtsIds.Count(); i += bulkReadSize)
                {
                    var subList = asgmtsIds.Skip(i).Take(bulkReadSize).ToArray();
                    var bulkRecords = await DataReader.BulkReadRecordAsync<PacLpAsgmts>(subList);
                    if (bulkRecords != null)
                    {
                        pacLpAsgmtsRecords.AddRange(bulkRecords);
                    }
                }
            }
            return pacLpAsgmtsRecords;
        }

        private async Task<DataContracts.HrssDefaults> GetHrssDefaultAsync()
        {

            var hrssDefaults = await DataReader.ReadRecordAsync<Data.HumanResources.DataContracts.HrssDefaults>("HR.PARMS", "HRSS.DEFAULTS");

            if (hrssDefaults == null)
            {
                var message = "Unable to find HrWebDefaults record";
                logger.Error(message);
                throw new KeyNotFoundException(message);
            };

            return hrssDefaults;
        }
    }
}
