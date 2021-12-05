/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{

    public class TestSupervisorsRepository : ISupervisorsRepository
    {
        #region  PERPOS
        public PerposRecord perposRecord1 = new PerposRecord
        {
            RecordKey = "001",
            PerposAltSupervisorId = null,
            PerposEndDate = null,
            PerposHrpId = "24601",
            PerposPositionId = "TMA001",
            PerposStartDate = new DateTime(1805, 12, 03),
            PerposSupervisorHrpId = "ANDRE3000"
        };
        public PerposRecord perposRecord2 = new PerposRecord
        {
            RecordKey = "002",
            PerposAltSupervisorId = null,
            PerposEndDate = null,
            PerposHrpId = "23",
            PerposPositionId = "TMA001",
            PerposStartDate = new DateTime(1806, 03, 12),
            PerposSupervisorHrpId = "ANDRE3000"
        };

        public PerposRecord perposRecord3 = new PerposRecord
        {
            RecordKey = "003",
            PerposAltSupervisorId = null,
            PerposEndDate = new DateTime(2000, 2, 29),
            PerposHrpId = "1492",
            PerposPositionId = "TMA004",
            PerposStartDate = new DateTime(1812, 2, 29),
            PerposSupervisorHrpId = "24601"
        };

        public PerposRecord perposRecord4 = new PerposRecord
        {
            RecordKey = "004",
            PerposAltSupervisorId = null,
            PerposEndDate = null,
            PerposHrpId = "45",
            PerposPositionId = "TMA004",
            PerposStartDate = new DateTime(1812, 2, 29),
            PerposSupervisorHrpId = ""
        };

        public PerposRecord perposRecord5 = new PerposRecord
        {
            RecordKey = "005",
            PerposAltSupervisorId = null,
            PerposEndDate = null,
            PerposHrpId = "08",
            PerposPositionId = "TMA004",
            PerposStartDate = new DateTime(1812, 2, 29),
            PerposSupervisorHrpId = "ANDRE3000"
        };

        public PerposRecord perposRecord6 = new PerposRecord
        {
            RecordKey = "006",
            PerposAltSupervisorId = null,
            PerposEndDate = new DateTime(2020, 01, 01),
            PerposHrpId = "24601",
            PerposPositionId = "TMA002",
            PerposStartDate = new DateTime(1812, 2, 29),
            PerposSupervisorHrpId = "ANDRE3000"
        };

        public PerposRecord perposRecord7 = new PerposRecord
        {
            RecordKey = "007",
            PerposAltSupervisorId = null,
            PerposEndDate = null,
            PerposHrpId = "46",
            PerposPositionId = "TMA003",
            PerposStartDate = new DateTime(1812, 2, 29),
            PerposSupervisorHrpId = ""
        };

        public List<PerposRecord> PerposRecords
        {
            get
            {
                return new List<PerposRecord>
                {
                    perposRecord1,
                    perposRecord2,
                    perposRecord3,
                    perposRecord4,
                    perposRecord5,
                    perposRecord6,
                    perposRecord7
                };
            }
        }
        #endregion

        #region POSITION
        public PositionRecord positionRecord1 = new PositionRecord
        {
            Recordkey = "TMA001",
            PosDept = "ECON",
            PosLocation = "WEST",
            AllPospay = new List<string> { "A", "B", "C" },
            PosAltSuperPosId = null,
            PosEndDate = null,
            PosExemptOrNot = "Y",
            PosHrlyOrSlry = "H",
            PosShortTitle = "Bandersnatch",
            PosStartDate = new DateTime(1800, 01, 01),
            PosSupervisorPosId = null,
            PosTitle = "Frumious Bandersnatch",
            TimeEntryType = "Detail"
        };

        public PositionRecord positionRecord2 = new PositionRecord
        {
            Recordkey = "TMA004",
            PosDept = "COMPSCI",
            PosLocation = "MAIN",
            AllPospay = new List<string> { "A", "B", "C" },
            PosAltSuperPosId = null,
            PosEndDate = null,
            PosExemptOrNot = "Y",
            PosHrlyOrSlry = "H",
            PosShortTitle = "Bird",
            PosStartDate = new DateTime(1802, 02, 02),
            PosSupervisorPosId = "TMA001",
            PosTitle = "Jubjub Bird",
            TimeEntryType = "Summary"
        };

        public PositionRecord positionRecord3 = new PositionRecord
        {
            Recordkey = "TMA002",
            PosDept = "ECON",
            PosLocation = "WEST",
            AllPospay = new List<string> { "A", "B", "C" },
            PosAltSuperPosId = null,
            PosEndDate = null,
            PosExemptOrNot = "Y",
            PosHrlyOrSlry = "H",
            PosShortTitle = "Bandersnatch 1",
            PosStartDate = new DateTime(2000, 01, 01),
            PosSupervisorPosId = null,
            PosTitle = "Frumious Bandersnatch 1",
            TimeEntryType = "Detail"
        };

        public PositionRecord positionRecord4 = new PositionRecord
        {
            Recordkey = "TMA003",
            PosDept = "ECON",
            PosLocation = "WEST",
            AllPospay = new List<string> { "A", "B", "C" },
            PosAltSuperPosId = null,
            PosEndDate = null,
            PosExemptOrNot = "Y",
            PosHrlyOrSlry = "H",
            PosShortTitle = "Bandersnatch 2",
            PosStartDate = new DateTime(2000, 01, 01),
            PosSupervisorPosId = "TMA002",
            PosTitle = "Frumious Bandersnatch 2",
            TimeEntryType = "Detail"
        };

        public List<PositionRecord> PositionRecords
        {
            get
            {
                return new List<PositionRecord>
                {
                    positionRecord1,
                    positionRecord2,
                    positionRecord3,
                    positionRecord4
                };
            }
        }
        #endregion

        public async Task<IEnumerable<string>> GetSupervisorsBySuperviseeAsync(string personId, DateTime? lookupStartDate = null)
        {
            var allSupervisors = new List<string>();
            var directSupervisorPerpos = new List<string>();
            var positionSupervisorPerpos = new List<string>();
            // no lookup start date specified
            if (!lookupStartDate.HasValue)
            {
                // this person's direct supervisors
                directSupervisorPerpos = this.PerposRecords.Where(x => x.PerposHrpId == personId).Select(x => x.PerposSupervisorHrpId).Distinct().ToList();
                allSupervisors.AddRange(directSupervisorPerpos);

                // this person's position level supervisors
                positionSupervisorPerpos = this.PerposRecords.Where(x => x.PerposHrpId == personId && string.IsNullOrWhiteSpace(x.PerposSupervisorHrpId)).Select(x => x.PerposPositionId).Distinct().ToList();
                foreach (var superPosition in positionSupervisorPerpos)
                {
                    var peopleWithThesePositions = this.PerposRecords.Where(x => x.PerposPositionId == superPosition).Select(x => x.PerposHrpId);
                    allSupervisors.AddRange(peopleWithThesePositions);
                }
            }
            // lookup start date specified
            else
            {
                // this person's direct supervisors
                directSupervisorPerpos = this.PerposRecords.Where(x => x.PerposHrpId == personId && (!x.PerposEndDate.HasValue || x.PerposEndDate.Value >= lookupStartDate.Value)).Select(x => x.PerposSupervisorHrpId).Distinct().ToList();
                allSupervisors.AddRange(directSupervisorPerpos);

                // this person's position level supervisors
                positionSupervisorPerpos = this.PerposRecords.Where(x => x.PerposHrpId == personId && string.IsNullOrWhiteSpace(x.PerposSupervisorHrpId) && (!x.PerposEndDate.HasValue || x.PerposEndDate.Value >= lookupStartDate.Value)).Select(x => x.PerposPositionId).Distinct().ToList();
                foreach (var superPosition in positionSupervisorPerpos)
                {
                    var peopleWithThesePositions = this.PerposRecords.Where(x => x.PerposPositionId == superPosition).Select(x => x.PerposHrpId);
                    allSupervisors.AddRange(peopleWithThesePositions);
                }
            }

            return await Task.FromResult(allSupervisors.Distinct());
        }

        public async Task<IEnumerable<string>> GetSuperviseesBySupervisorAsync(string personId, DateTime? lookupStartDate = default(DateTime?))
        {
            var direct = new List<string>();
            var supervisorPerpos = new List<string>();
            var subordinatePositions = new List<PositionRecord>();
            var subordinatePerpos = new List<PerposRecord>();
            if (!lookupStartDate.HasValue)
            {
                // direct
                direct = this.PerposRecords.Where(x => x.PerposSupervisorHrpId == personId).Select(x => x.PerposHrpId).Distinct().ToList();

                // position
                supervisorPerpos = this.PerposRecords.Where(x => x.PerposHrpId == personId).Select(x => x.PerposPositionId).Distinct().ToList();
                subordinatePositions = this.PositionRecords.Where(x => supervisorPerpos.Any(y => y == x.PosSupervisorPosId)).Distinct().ToList();
                subordinatePerpos = this.PerposRecords.Where(x => subordinatePositions.Any(y => y.Recordkey == x.PerposPositionId &&
                string.IsNullOrWhiteSpace(x.PerposSupervisorHrpId))).Distinct().ToList();
            }
            else
            {
                // direct
                direct = this.PerposRecords.Where(x => x.PerposSupervisorHrpId == personId && (!x.PerposEndDate.HasValue || x.PerposEndDate.Value >= lookupStartDate.Value))
                    .Select(x => x.PerposHrpId).Distinct().ToList();

                // position
                supervisorPerpos = this.PerposRecords.Where(x => x.PerposHrpId == personId && (!x.PerposEndDate.HasValue || x.PerposEndDate.Value >= lookupStartDate.Value))
                    .Select(x => x.PerposPositionId).Distinct().ToList();
                subordinatePositions = this.PositionRecords.Where(x => supervisorPerpos.Any(y => y == x.PosSupervisorPosId )).Distinct().ToList();
                subordinatePerpos = this.PerposRecords.Where(x => subordinatePositions.Any(y => y.Recordkey == x.PerposPositionId &&
                string.IsNullOrWhiteSpace(x.PerposSupervisorHrpId) && (!x.PerposEndDate.HasValue || x.PerposEndDate.Value >= lookupStartDate.Value))).Distinct().ToList();
            }
            var position = subordinatePerpos.Select(x => x.PerposHrpId).Distinct().ToList();

            var subordinates = new List<string>();
            subordinates.AddRange(direct);
            subordinates.AddRange(position);

            return await Task.FromResult(subordinates);
        }

        public Task<Dictionary<string, List<string>>> GetSupervisorIdsForPositionsAsync(IEnumerable<string> positionIds)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetSupervisorsByPositionIdAsync(string positionId, string superviseeId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetSuperviseesByPrimaryPositionForSupervisorAsync(string supervisorId)
        {
            throw new NotImplementedException();
        }
    }

    public class InternationalParameters
    {
        public string HostShortDateFormat { get; set; }
        public string HostDateDelimiter { get; set; }
    }

    public class PerposRecord
    {
        public string RecordKey { get; set; }
        public string PerposHrpId { get; set; }
        public string PerposPositionId { get; set; }
        public DateTime? PerposStartDate { get; set; }
        public DateTime? PerposEndDate { get; set; }
        public string PerposSupervisorHrpId { get; set; }
        public string PerposAltSupervisorId { get; set; }

    }

    public class PositionRecord
    {
        public string Recordkey { get; set; }
        public string PosTitle { get; set; }
        public string PosDept { get; set; }
        public string PosLocation { get; set; }
        public DateTime? PosEndDate { get; set; }
        public string PosHrlyOrSlry { get; set; }
        public string PosExemptOrNot { get; set; }
        public string PosShortTitle { get; set; }
        public List<string> AllPospay { get; set; }
        public DateTime? PosStartDate { get; set; }
        public string PosSupervisorPosId { get; set; }
        public string PosAltSuperPosId { get; set; }
        public string TimeEntryType { get; set; }

    }

}
