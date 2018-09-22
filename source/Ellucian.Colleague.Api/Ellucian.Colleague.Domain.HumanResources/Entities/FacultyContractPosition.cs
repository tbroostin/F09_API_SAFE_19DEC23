/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    [Serializable]
    public class FacultyContractPosition
    {
        public string Id { get; private set; }

        public string LoadPeriodId { get; private set; }

        public decimal? IntendedLoad { get; private set; }

        public string PositionId { get; private set; }

        public List<FacultyContractAssignment> FacultyContractAssignments { get; private set; }

        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value ?? string.Empty;
            }
        }

        private string title;
        // Member assignment list instead of assignment Campus.org.member.asgmt
        public FacultyContractPosition(string id, string loadPeriodId, decimal? intendedLoad, string positionId)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "id cannot be null or empty");
            }
            if (string.IsNullOrEmpty(loadPeriodId))
            {
                throw new ArgumentNullException("loadPeriodId", "loadPeriodId cannot be null or empty");
            }
            Id = id;
            LoadPeriodId = loadPeriodId;
            IntendedLoad = intendedLoad;
            FacultyContractAssignments = new List<FacultyContractAssignment>();
            Title = string.Empty;
            PositionId = positionId;
        }
    }
}