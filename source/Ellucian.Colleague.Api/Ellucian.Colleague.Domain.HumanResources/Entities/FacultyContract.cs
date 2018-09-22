/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    [Serializable]
    public class FacultyContract
    {
        public string Id { get; private set; }
        public string ContractDescription { get; private set; }
        public string ContractNumber { get; private set; }
        public string ContractType { get; set; }
        public DateTime? StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public string LoadPeriodId { get; private set; }
        public decimal? IntendedTotalLoad { get; private set; }
        public decimal? TotalValue { get; private set; }
        public List<FacultyContractPosition> FacultyContractPositions { get; private set; }

        public FacultyContract(string id, string desc, string number, string type, DateTime? startDate, DateTime? endDate, string loadPeriodId, decimal? intendedTotalLoad, decimal? totalValue)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "id cannot be null or empty");
            }
            if (string.IsNullOrEmpty(desc))
            {
                throw new ArgumentNullException("desc", "description cannot be null or empty");

            }
            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentNullException("type", "type cannot be null or empty");

            }
            if (startDate == null)
            {
                throw new ArgumentNullException("startDate", "start date cannot be null");
            }
            if (string.IsNullOrEmpty(loadPeriodId))
            {
                throw new ArgumentNullException("loadPeriodId", "load period id cannot be null or empty");
            }


            Id = id;
            ContractDescription = desc;
            ContractNumber = number;
            ContractType = type;
            StartDate = startDate;
            EndDate = endDate;
            LoadPeriodId = loadPeriodId;
            IntendedTotalLoad = intendedTotalLoad;
            TotalValue = totalValue;
            FacultyContractPositions = new List<FacultyContractPosition>();
          
        }
    }
}
